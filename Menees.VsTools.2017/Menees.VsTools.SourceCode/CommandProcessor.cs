namespace Menees.VsTools
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading.Tasks;
	using EnvDTE;
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;
	using VSLangProj;

	#endregion

	internal sealed class CommandProcessor
	{
		#region Private Data Members

		private static readonly Guid StartPageKind = new Guid("{387CB18D-6153-4156-9257-9AC3F9207BBE}");

		private MainPackage package;
		private DTE dte;
		private Options options;
		private WindowEvents windowEvents;

		#endregion

		#region Constructors

		public CommandProcessor(MainPackage package)
		{
			this.package = package;
			this.dte = (DTE)this.GetService(typeof(SDTE));
			this.options = this.package.Options;
			this.options.Applied += (s, e) => this.ApplyOptions();
			this.ApplyOptions();
		}

		#endregion

		#region Private Properties

		private Language ActiveLanguage
		{
			get
			{
				Language result = Utilities.GetLanguage(this.dte.ActiveDocument);
				return result;
			}
		}

		#endregion

		#region Public Methods

		public bool CanExecute(Command command)
		{
			try
			{
				bool result = false;

				if (this.dte != null)
				{
					switch (command)
					{
						// These require a text selection.
						case Command.SortLines:
						case Command.Trim:
						case Command.Statistics:
						case Command.StreamText:
						case Command.ExecuteText:
						case Command.CheckSpelling:
							result = new TextDocumentHandler(this.dte).HasNonEmptySelection;
							break;

						// These require a text selection for specific languages.
						case Command.CommentSelection:
						case Command.UncommentSelection:
							result = CommentHandler.CanCommentSelection(this.dte, command == Command.CommentSelection);
							break;

						// These require a document using a supported language.
						case Command.AddRegion:
						case Command.CollapseAllRegions:
						case Command.ExpandAllRegions:
							result = RegionHandler.IsSupportedLanguage(this.ActiveLanguage);
							break;

						// These require an open document with a backing file on disk.
						case Command.ExecuteFile:
						case Command.ToggleReadOnly:
							string fileName = this.GetDocumentFileName();
							result = File.Exists(fileName);
							break;

						case Command.GenerateGuid:
							result = new TextDocumentHandler(this.dte).CanSetSelectedText;
							break;

						case Command.ToggleFiles:
							result = ToggleFilesHandler.IsSupportedLanguage(this.ActiveLanguage);
							break;

						case Command.ListAllProjectProperties:
							result = ProjectHandler.GetSelectedProjects(this.dte, null);
							break;

						case Command.ViewBaseConverter:
						case Command.ViewTasks:
							result = true;
							break;

						case Command.SortMembers:
							result = new MemberSorter(this.dte, false).CanFindMembers;
							break;

						case Command.AddToDoComment:
							result = CommentHandler.CanAddToDoComment(this.dte);
							break;
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				MainPackage.LogException(ex);
				throw;
			}
		}

		public void Execute(Command command)
		{
			try
			{
				if (this.dte != null)
				{
					switch (command)
					{
						case Command.AddRegion:
							RegionHandler.AddRegion(this.dte, Options.SplitValues(this.package.Options.PredefinedRegions));
							break;

						case Command.CheckSpelling:
							this.CheckSpelling();
							break;

						case Command.CollapseAllRegions:
							RegionHandler.CollapseAllRegions(this.dte, this.ActiveLanguage, this.package);
							break;

						case Command.CommentSelection:
						case Command.UncommentSelection:
							CommentHandler.CommentSelection(this.dte, this.package, command == Command.CommentSelection);
							break;

						case Command.ExecuteFile:
							this.ExecuteFile();
							break;

						case Command.ExecuteText:
							this.ExecuteText();
							break;

						case Command.ExpandAllRegions:
							RegionHandler.ExpandAllRegions(this.dte, this.ActiveLanguage);
							break;

						case Command.GenerateGuid:
							this.GenerateGuid();
							break;

						case Command.ListAllProjectProperties:
							ProjectHandler.ListAllProjectProperties(this.dte);
							break;

						case Command.SortLines:
							this.SortLines();
							break;

						case Command.Statistics:
							this.Statistics();
							break;

						case Command.StreamText:
							this.StreamText();
							break;

						case Command.ToggleFiles:
							ToggleFilesHandler toggleFilesHandler = new ToggleFilesHandler(this.dte, this.package);
							toggleFilesHandler.ToggleFiles();
							break;

						case Command.ToggleReadOnly:
							this.ToggleReadOnly();
							break;

						case Command.Trim:
							this.Trim();
							break;

						case Command.ViewBaseConverter:
							this.ViewToolWindow(typeof(BaseConverterWindow));
							break;

						case Command.SortMembers:
							this.SortMembers();
							break;

						case Command.AddToDoComment:
							CommentHandler.AddToDoComment(this.dte);
							break;

						case Command.ViewTasks:
							this.ViewToolWindow(typeof(Tasks.TasksWindow));
							break;
					}
				}
			}
			catch (Exception ex)
			{
				MainPackage.LogException(ex);
				throw;
			}
		}

		#endregion

		#region Private Methods

		private object GetService(Type serviceType)
		{
			object result = this.package.ServiceProvider.GetService(serviceType);
			return result;
		}

		private string GetDocumentFileName()
		{
			Document doc = this.dte.ActiveDocument;
			string result = doc?.FullName;
			return result;
		}

		private void CheckSpelling()
		{
			TextDocumentHandler handler = new TextDocumentHandler(this.dte);
			if (handler.HasNonEmptySelection)
			{
				try
				{
					// Launch Word.
					Word._Application wordApp = new Word.Application();

					// Add a document.
					Word._Document wordDoc = wordApp.Documents.Add();

					// Clear current contents.
					Word.Range range = wordApp.Selection.Range;
					range.WholeStory();
					range.Delete();
					range = null;

					// Add the text the user selected.
					wordApp.Selection.Text = handler.SelectedText;

					// Show it
					wordApp.Visible = true;
					wordApp.Activate();
					wordDoc.Activate();

					// Check spelling
					wordDoc.CheckSpelling();

					// Get the edited text back
					wordApp.Selection.WholeStory();
					string newText = wordApp.Selection.Text;

					// Word always adds an extra CR, so strip that off.
					// Also it converts all LFs to CRs, so change
					// that back.
					if (!string.IsNullOrEmpty(newText))
					{
						if (newText.EndsWith("\r"))
						{
							newText = newText.Substring(0, newText.Length - 1);
						}

						newText = newText.Replace("\r", "\r\n");
					}

					handler.SetSelectedTextIfUnchanged(newText, "Check Spelling");

					// Tell the doc and Word to go away.
					object saveChanges = false;
					wordDoc.Close(ref saveChanges);
					wordApp.Visible = false;
					wordApp.Quit();
				}
				catch (COMException ex)
				{
					// If we get REGDB_E_CLASSNOTREG, then Word probably isn't installed.
					const uint REGDB_E_CLASSNOTREG = 0x80040154;
					if (unchecked((uint)ex.ErrorCode) == REGDB_E_CLASSNOTREG)
					{
						this.package.ShowMessageBox(
							"Microsoft Word is required in order to check spelling, but it isn't available.\r\n\r\nDetails:\r\n" + ex.Message,
							true);
					}
					else
					{
						throw;
					}
				}
			}
		}

		private void ExecuteFile()
		{
			// Perform the SaveAll first (if necessary) in case the "executing" file has never
			// been saved before, so this will assign it a file name.
			bool performExecute = true;
			Documents allDocs = this.dte.Documents;
			if (allDocs != null && this.package.Options.SaveAllBeforeExecuteFile)
			{
				try
				{
					allDocs.SaveAll();
				}
				catch (ExternalException ex)
				{
					performExecute = false;

					// Rethrow the exception unless the user hit Cancel when prompted to save changes for a document.
					// Cancelling throws an HRESULT of 0x80004004, which is the C constant E_ABORT with the description
					// "Operation aborted".
					const uint E_ABORT = 0x80004004;
					if (unchecked((uint)ex.ErrorCode) != E_ABORT)
					{
						throw;
					}
				}
			}

			if (performExecute)
			{
				string fileName = this.GetDocumentFileName();
				if (!string.IsNullOrEmpty(fileName))
				{
					Utilities.ShellExecute(fileName);
				}
			}
		}

		private void ExecuteText()
		{
			TextDocumentHandler handler = new TextDocumentHandler(this.dte);
			if (handler.HasNonEmptySelection)
			{
				Utilities.ShellExecute(handler.SelectedText);
			}
		}

		private void GenerateGuid()
		{
			TextDocumentHandler handler = new TextDocumentHandler(this.dte);
			if (handler.CanSetSelectedText)
			{
				Guid guid = Guid.NewGuid();
				Options options = this.package.Options;

				string format;
				switch (options.GuidFormat)
				{
					case GuidFormat.Numbers:
						format = "N";
						break;

					case GuidFormat.Braces:
						format = "B";
						break;

					case GuidFormat.Parentheses:
						format = "P";
						break;

					case GuidFormat.Structure:
						format = "X";
						break;

					default: // GuidFormat.Dashes
						format = "D";
						break;
				}

				string guidText = guid.ToString(format);
				if (options.UppercaseGuids)
				{
					guidText = guidText.ToUpper();
				}

				// Set the selection to the new GUID
				handler.SetSelectedText(guidText, "Generate GUID");
			}
		}

		private void SortLines()
		{
			TextDocumentHandler handler = new TextDocumentHandler(this.dte);
			if (handler.HasNonEmptySelection)
			{
				Options options = this.package.Options;

				SortDialog dialog = new SortDialog();
				if (dialog.Execute(options))
				{
					StringComparison comparison;
					if (options.SortCompareByOrdinal)
					{
						comparison = options.SortCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
					}
					else
					{
						comparison = options.SortCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
					}

					// Now sort the lines and put them back as the selection
					string text = handler.SelectedText;
					TextLines lines = new TextLines(text);
					lines.Sort(comparison, options.SortAscending, options.SortIgnoreWhitespace, options.SortIgnorePunctuation, options.SortEliminateDuplicates);
					string sortedText = lines.ToString();
					handler.SetSelectedTextIfUnchanged(sortedText, "Sort Lines");
				}
			}
		}

		private void SortMembers()
		{
			MemberSorter sorter = new MemberSorter(this.dte, true);
			if (sorter.HasSelectedMembers)
			{
				Options options = this.package.Options;
				sorter.SortMembers(options);
			}
		}

		private void Statistics()
		{
			TextDocumentHandler handler = new TextDocumentHandler(this.dte);
			if (handler.HasNonEmptySelection)
			{
				string text = handler.SelectedText;
				StatisticsDialog dialog = new StatisticsDialog();
				dialog.Execute(text);
			}
		}

		private void StreamText()
		{
			TextDocumentHandler handler = new TextDocumentHandler(this.dte);
			if (handler.HasNonEmptySelection)
			{
				string text = handler.SelectedText;
				TextLines lines = new TextLines(text);
				string streamedText = lines.Stream();
				handler.SetSelectedTextIfUnchanged(streamedText, "Stream Text");
			}
		}

		private void ToggleReadOnly()
		{
			string fileName = this.GetDocumentFileName();
			if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
			{
				FileAttributes attr = File.GetAttributes(fileName);

				if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					attr = attr & ~FileAttributes.ReadOnly;
				}
				else
				{
					attr = attr | FileAttributes.ReadOnly;
				}

				File.SetAttributes(fileName, attr);
			}
		}

		private void Trim()
		{
			TextDocumentHandler handler = new TextDocumentHandler(this.dte);
			if (handler.HasNonEmptySelection)
			{
				Options options = this.package.Options;

				bool execute = true;
				if (!options.OnlyShowTrimDialogWhenShiftIsPressed || Utilities.IsShiftPressed)
				{
					TrimDialog dialog = new TrimDialog();
					execute = dialog.Execute(options);
				}

				if (execute && (options.TrimStart || options.TrimEnd))
				{
					string text = handler.SelectedText;
					TextLines lines = new TextLines(text);
					lines.Trim(options.TrimStart, options.TrimEnd);
					string trimmedText = lines.ToString();
					handler.SetSelectedTextIfUnchanged(trimmedText, "Trim");
				}
			}
		}

		private void ViewToolWindow(Type toolWindowPaneType)
		{
			// Get instance number 0 of this tool window. It's single instance so that's the only one.
			// The last flag is set to true so that if the tool window does not exist it will be created.
			ToolWindowPane window = this.package.FindToolWindow(toolWindowPaneType, 0, true);
			if ((window == null) || (window.Frame == null))
			{
				throw new NotSupportedException(Properties.Resources.CannotCreateWindow);
			}

			IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
			ErrorHandler.ThrowOnFailure(windowFrame.Show());
		}

		private void ApplyOptions()
		{
			if (this.options.AutoCloseStartPage)
			{
				// We must hold a reference to the returned WindowEvents instance.  Otherwise, we won't get callbacks/events.
				// See comments in BuildTimer.cs constructor.
				this.windowEvents = this.dte.Events.WindowEvents;
				this.windowEvents.WindowActivated += this.WindowEvents_WindowActivated;
			}
			else if (this.windowEvents != null)
			{
				this.windowEvents.WindowActivated -= this.WindowEvents_WindowActivated;
				this.windowEvents = null;
			}
		}

		private void WindowEvents_WindowActivated(Window gotFocus, Window lostFocus)
		{
			// We might could check the VS setting in this.dte.Properties["Environment", "Startup"],
			// but there's little point in it.  We just need a bool for our purpose.
			// https://social.msdn.microsoft.com/Forums/vstudio/en-US/4f59de7c-715e-4f42-93d4-5e13efd626e3 (...)
			// 			/visual-studio-2017-disable-start-page?forum=visualstudiogeneral
			// Could also use IVsSolutionEvents like:
			// https://github.com/jlattimer/VSReopenStartPage/blob/master/VSReopenStartPage/VsSolutionEvents.cs
			if (this.options.AutoCloseStartPage && Guid.TryParse(gotFocus.ObjectKind, out Guid focusedKind) && focusedKind == StartPageKind)
			{
				Debug.WriteLine("Start Page activated");

				// We can't immediately call gotFocus.Close() during the WindowActivated handler.
				System.Threading.Tasks.Task.Run(() =>
				{
					bool closed = false;
					const int MaxAttempts = 5;
					for (int attempt = 1; attempt <= MaxAttempts; attempt++)
					{
						try
						{
							gotFocus.Close();
							closed = true;
							Debug.WriteLine("Start Page closed");
							break;
						}
						catch (InvalidOperationException)
						{
							Debug.WriteLine($"Start Page close attempt {attempt} failed");
							System.Threading.Thread.Sleep(attempt);
						}
					}

					if (!closed && gotFocus.Visible && this.dte.ActiveWindow == gotFocus)
					{
						// If we use IVsUIShell.PostExecCommand to invoke the Window.CloseToolWindow
						// command, it usually works, but we can't specify an argument.  So there's a race
						// condition around focus change, which sucks.
						// Based on example from http://stackoverflow.com/a/20243377/1882616.
						Debug.WriteLine("Attempting to close Start Page via Window.CloseToolWindow");
						IVsUIShell shell = (IVsUIShell)this.GetService(typeof(SVsUIShell));
						var command = this.dte.Commands.Item("Window.CloseToolWindow", 0);
						Guid guid = new Guid(command.Guid);
						object input = null;
						int hr = shell.PostExecCommand(ref guid, (uint)command.ID, 0, ref input);
						if (ErrorHandler.Failed(hr))
						{
							Debug.WriteLine("Could not close Start Page tool window");
						}
					}
				});
			}
		}

		#endregion
	}
}
