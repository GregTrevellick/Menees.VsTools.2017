namespace Menees.VsTools
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading.Tasks;
	using EnvDTE;

	#endregion

	internal sealed class TextDocumentHandler
	{
		#region Public Constants

		public const string UndoContextSuffix = " (" + MainPackage.Title + ")";

		#endregion

		#region Private Data Members

		private DTE dte;
		private Document document;
		private TextDocument textDocument;
		private TextSelection selection;

		#endregion

		#region Constructors

		public TextDocumentHandler(DTE dte)
		{
			this.dte = dte;

			this.document = dte.ActiveDocument;
			if (this.document != null)
			{
				try
				{
					this.textDocument = this.document.Object("TextDocument") as TextDocument;
					if (this.textDocument != null)
					{
						this.selection = this.textDocument.Selection;
					}
				}
				catch (COMException)
				{
					// Ignore this.  If a non-text document is open (e.g., the WinForms designer),
					// then calling document.Object("TextDocument") or textDocument.Selection
					// can raise a COMException.
				}
			}
		}

		#endregion

		#region Public Properties

		public bool CanSetSelectedText
		{
			get
			{
				bool result = this.selection != null;
				return result;
			}
		}

		public bool HasNonEmptySelection
		{
			get
			{
				bool result;
				try
				{
					result = this.selection != null && !this.selection.IsEmpty;
				}
				catch (COMException)
				{
					// In VS 2017 the "new" HTML editor (i.e., not the old Web Forms one) will sometimes throw
					// (0x80004005): Unspecified error (Exception from HRESULT: 0x80004005 (E_FAIL)).
					result = false;
				}

				return result;
			}
		}

		public string SelectedText
		{
			get
			{
				string result = null;

				if (this.selection != null)
				{
					result = this.selection.Text;
				}

				return result;
			}
		}

		public Document Document => this.document;

		public TextDocument TextDocument => this.textDocument;

		public Language Language
		{
			get
			{
				Language result = Utilities.GetLanguage(this.document);
				return result;
			}
		}

		public TextSelection Selection => this.selection;

		#endregion

		#region Public Methods

		public void SetSelectedText(string newText, string commandName)
		{
			this.SetSelectedText(newText, false, commandName);
		}

		public void SetSelectedTextIfUnchanged(string newText, string commandName)
		{
			// Check for differences, and if there are no changes then don't overwrite
			// the selection.  This keeps the document from being marked Modified if
			// no modifications have been made.
			if (this.SelectedText != newText)
			{
				this.SetSelectedText(newText, true, commandName);
			}
		}

		public void Execute(string commandName, Action commandAction)
		{
			// Put all of the changes in a single Undo context.
			UndoContext undo = this.dte.UndoContext;
			bool undoAlreadyOpen = undo.IsOpen;

			if (!undoAlreadyOpen)
			{
				if (string.IsNullOrEmpty(commandName))
				{
					commandName = "Set Selected Text";
				}

				undo.Open(commandName + UndoContextSuffix, false);
			}

			try
			{
				commandAction();
			}
			catch (Exception ex)
			{
				// Most commands won't change anything until they try to push in the final SelectedText value.
				// But some commands (e.g., Sort Members) need to update things in intermediate steps (e.g.,
				// removing members from the editor's code model).  If any exception occurs, we need to abort
				// everything that was done in the current context to avoid data loss (e.g., losing the members
				// that were temporarily removed from the editor's code model).
				undo.SetAborted();

				// If the file is read-only (e.g., in source control) and the user cancels the edit,
				// then a COMException is thrown with HRESULT 0x80041005 (WBEM_E_TYPE_MISMATCH).
				// We'll ignore that exception, but we'll re-throw anything else.
				const uint WBEM_E_TYPE_MISMATCH = 0x80041005;
				if (!(ex is COMException) || ex.HResult != unchecked((int)WBEM_E_TYPE_MISMATCH))
				{
					throw;
				}
			}
			finally
			{
				// Make sure we close the UndoContext.
				if (!undoAlreadyOpen && !undo.IsAborted)
				{
					undo.Close();
				}
			}
		}

		#endregion

		#region Private Methods

		private void SetSelectedText(string newText, bool keepSelection, string commandName)
		{
			if (this.selection == null)
			{
				throw new InvalidOperationException("SetSelectedText requires a non-null selection.");
			}

			this.Execute(
				commandName,
				() =>
				{
					// Delete any existing selected text.
					if (!this.selection.IsEmpty)
					{
						// If we pass zero here, then nothing happens.
						// It appears to work fine (i.e. deleting the
						// entire selection) if we pass any non-zero value.
						this.selection.Delete(1);
					}

					// Insert the new text, optionally drop the selection,
					// and leave the caret at the end.
					if (keepSelection)
					{
						this.selection.Insert(newText, (int)vsInsertFlags.vsInsertFlagsInsertAtStart);
						this.selection.SwapAnchor();
					}
					else
					{
						this.selection.Insert(newText, (int)vsInsertFlags.vsInsertFlagsCollapseToEnd);
					}
				});
		}

		#endregion
	}
}
