namespace Menees.VsTools.Editor
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Text;
	using Microsoft.VisualStudio.Text.Classification;
	using Microsoft.VisualStudio.Text.Editor;
	using Microsoft.VisualStudio.Text.Projection;
	using Microsoft.VisualStudio.TextManager.Interop;
	using Microsoft.VisualStudio.Utilities;
	using Microsoft.Win32;
	using Tasks;
	#endregion

	/// <summary>
	/// Classifies text as an instance of the <see cref="HighlightFormat.HighlightType"/>.
	/// </summary>
	internal sealed class HighlightClassifier : ClassifierBase
	{
		#region Private Data Members

		private const string InsertTabsOptionName = "Insert Tabs";
		private const string TabSizeOptionName = "Tab Size";

		private static readonly Lazy<Dictionary<string, Guid>> LanguageGuids = new Lazy<Dictionary<string, Guid>>(CacheLanguageGuids);

		private IClassificationType classificationType;
		private IEditorOptions editorOptions;
		private bool convertTabsToSpaces;
		private int tabSize;
		private bool isCodeContentType;
		private string fileExtension;
		private bool isHighlightedFileExtension;
		private string textManagerLanguageName;

		#endregion

		#region Constructors

		internal HighlightClassifier(
			ITextBuffer buffer,
			IClassificationType classificationType,
			IEditorOptions editorOptions,
			ITextDocumentFactoryService textDocumentFactory)
			: base(buffer)
		{
			this.classificationType = classificationType;
			this.editorOptions = editorOptions;
			this.SetContentType(buffer, buffer.ContentType);
			this.ReadOptions(null);

			ITextDocument document;
			if (textDocumentFactory == null || !textDocumentFactory.TryGetTextDocument(buffer, out document))
			{
				document = Tasks.DocumentItem.GetTextDocument(buffer);
			}

			if (document != null)
			{
				document.FileActionOccurred += this.DocumentFileActionOccurred;
				this.SetFileExtension(document.FilePath);
			}

			// Indicate that we need to re-classify everything when there is a change to any of the editor options we depend on.
			this.editorOptions.OptionChanged += this.EditorOptionsChanged;
		}

		#endregion

		#region Protected Methods

		protected override void GetClassificationSpans(List<ClassificationSpan> result, SnapshotSpan span, Options options)
		{
			bool highlightContent = (this.isCodeContentType && options.HighlightAllCodeFiles) || this.isHighlightedFileExtension;
			if (highlightContent)
			{
				int excessLineLength = options.HighlightExcessLineLength;
				bool highlightExcessLineLength = excessLineLength > 0;
				bool highlightTrailingWhiteSpace = options.HighlightTrailingWhiteSpace;
				bool highlightInvalidLeadingWhiteSpace = options.HighlightInvalidLeadingWhiteSpace;

				if (highlightExcessLineLength || highlightTrailingWhiteSpace || highlightInvalidLeadingWhiteSpace)
				{
					foreach (ITextSnapshotLine line in GetSpanLines(span))
					{
						string text = line.GetText();
						if (!string.IsNullOrEmpty(text))
						{
							// Note: I could theoretically combine all these checks into a single pass.
							// Now the excess line check has to read most of the characters again.
							// But I'm going to keep this simple approach unless performance is bad.
							bool allWhiteSpace = false;
							if (highlightTrailingWhiteSpace)
							{
								allWhiteSpace = this.CheckTrailingWhiteSpace(result, line, text);
							}

							if (highlightInvalidLeadingWhiteSpace && !allWhiteSpace)
							{
								this.CheckLeadingWhiteSpace(result, line, text);
							}

							if (highlightExcessLineLength)
							{
								this.CheckExcessLength(result, line, text, excessLineLength);
							}
						}
					}
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			this.editorOptions.OptionChanged -= this.EditorOptionsChanged;
		}

		protected override bool ReadOptions(string changedOptionId)
		{
			bool allChanged = string.IsNullOrEmpty(changedOptionId);
			bool changed = false;

			if (allChanged || changedOptionId == DefaultOptions.ConvertTabsToSpacesOptionId.Name)
			{
				this.convertTabsToSpaces = this.editorOptions.GetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId);
				changed = true;

				if (!string.IsNullOrEmpty(this.textManagerLanguageName))
				{
					this.convertTabsToSpaces = this.GetTextManagerOptionValue(InsertTabsOptionName, this.convertTabsToSpaces ? 0 : 1) == 0;
				}
			}

			if (allChanged || changedOptionId == DefaultOptions.TabSizeOptionId.Name)
			{
				this.tabSize = this.editorOptions.GetOptionValue(DefaultOptions.TabSizeOptionId);
				changed = true;

				if (!string.IsNullOrEmpty(this.textManagerLanguageName))
				{
					this.tabSize = this.GetTextManagerOptionValue(TabSizeOptionName, this.tabSize);
				}
			}

			if (allChanged)
			{
				this.UpdateIsHighlightedFileExtension();
			}

			return changed;
		}

		protected override void ContentTypeChanged(ITextBuffer buffer, ContentTypeChangedEventArgs e)
		{
			base.ContentTypeChanged(buffer, e);

			// Update the content type and force the options to be re-cached.
			this.SetContentType(buffer, e.AfterContentType);
			this.OptionsChanged(null);
		}

		#endregion

		#region Private Methods

		private static Dictionary<string, Guid> CacheLanguageGuids()
		{
			Dictionary<string, Guid> result = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

			if (MainPackage.Instance != null)
			{
				using (RegistryKey rootKey = MainPackage.Instance.UserRegistryRoot)
				{
					if (rootKey != null)
					{
						ScanInfo.ScanLanguageServices(
							rootKey,
							(langName, langGuid) =>
							{
								Guid guid;
								if (Guid.TryParse(langGuid, out guid))
								{
									result.Add(langName, guid);
								}

								return false;
							});
					}
				}
			}

			return result;
		}

		private void AddClassificationSpan(List<ClassificationSpan> result, SnapshotPoint startPoint, SnapshotPoint endPoint)
		{
			SnapshotSpan snapshotSpan = new SnapshotSpan(startPoint, endPoint);
			ClassificationSpan classificationSpan = new ClassificationSpan(snapshotSpan, this.classificationType);
			result.Add(classificationSpan);
		}

		private bool CheckTrailingWhiteSpace(List<ClassificationSpan> result, ITextSnapshotLine line, string text)
		{
			// Look for invalid trailing whitespace.
			int length = text.Length;
			int trailingWhiteSpaceEnds = length;
			for (int i = length - 1; i >= 0; i--)
			{
				if (!char.IsWhiteSpace(text[i]))
				{
					break;
				}

				trailingWhiteSpaceEnds--;
			}

			bool allWhiteSpace = trailingWhiteSpaceEnds < 0;
			if (trailingWhiteSpaceEnds < length)
			{
				SnapshotPoint startPoint = allWhiteSpace ? line.Start : line.End.Subtract(length - trailingWhiteSpaceEnds);
				this.AddClassificationSpan(result, startPoint, line.End);
			}

			return allWhiteSpace;
		}

		private void CheckLeadingWhiteSpace(List<ClassificationSpan> result, ITextSnapshotLine line, string text)
		{
			int index = 0;
			SnapshotPoint lineStart = line.Start;
			SnapshotPoint? startPoint = null;
			foreach (char ch in text)
			{
				// Quit if when we see anything other than a space or tab.
				if (ch != ' ' && ch != '\t')
				{
					break;
				}

				// See if we're starting or finishing a span of invalid whitespace characters.
				if ((ch == '\t' && this.convertTabsToSpaces) || (ch == ' ' && !this.convertTabsToSpaces))
				{
					// We need to keep the earliest start point in the current span.
					if (startPoint == null)
					{
						startPoint = lineStart.Add(index);
					}
				}
				else if (startPoint != null)
				{
					// We finished an invalid whitespace span, so add it.
					SnapshotPoint endPoint = lineStart.Add(index);
					this.AddClassificationSpan(result, startPoint.Value, endPoint);
					startPoint = null;
				}

				index++;
			}

			if (startPoint != null)
			{
				SnapshotPoint endPoint = index >= text.Length ? line.End : lineStart.Add(index);
				this.AddClassificationSpan(result, startPoint.Value, endPoint);
			}
		}

		private void CheckExcessLength(List<ClassificationSpan> result, ITextSnapshotLine line, string text, int maxLength)
		{
			// Even if this.convertTabsToSpaces is true, we can still encounter tabs that we need to expand
			// (e.g., if they loaded a file that contains tabs).  So we have to scan the whole line to calculate
			// the correct visible length (i.e., column-based) based on tabs expanded to the next tab stop.
			int visibleLength = 0;
			int textLength = text.Length;
			int textIndex;
			for (textIndex = 0; textIndex < textLength; textIndex++)
			{
				if (text[textIndex] == '\t')
				{
					// A tab always takes up at least one column and up to TabSize columns.
					// We just need to add the number of columns to get to the next tab stop.
					visibleLength += this.tabSize - (visibleLength % this.tabSize); // Always in [1, TabSize] range.
				}
				else
				{
					visibleLength++;
				}

				if (visibleLength > maxLength)
				{
					this.AddClassificationSpan(result, line.Start.Add(textIndex), line.End);
					break;
				}
			}
		}

		private void EditorOptionsChanged(object sender, EditorOptionChangedEventArgs e)
		{
			this.OptionsChanged(e.OptionId);
		}

		private void DocumentFileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
		{
			if (e.FileActionType.HasFlag(FileActionTypes.DocumentRenamed))
			{
				this.SetFileExtension(e.FilePath);

				// Pretend an option was changed so everything in the document will be reclassified.
				this.OptionsChanged(null);
			}
		}

		private void SetContentType(ITextBuffer buffer, IContentType contentType)
		{
			this.isCodeContentType = Utilities.IsContentOfType(contentType, "code");

			// In VS 2013-2017 the HTML language service doesn't provide the correct tabs/spaces options
			// from IEditorOptions, so we have to read them from IVsTextManager.  Unfortunately, this means
			// we won't get OptionChanged notifications for them.
			this.textManagerLanguageName = null;
			if (Utilities.IsContentOfType(contentType, "HTML"))
			{
				this.textManagerLanguageName = "HTML";
			}
			else if (Utilities.IsContentOfType(contentType, "HTMLX"))
			{
				this.textManagerLanguageName = "HTMLX";
			}
			else
			{
				// If an HTML editor has embedded sections using other languages (e.g., JavaScript, C#, Basic, CSS)
				// then the HTML editor will also give us incorrect IEditorOptions for them.
				IProjectionBuffer projection = buffer as IProjectionBuffer;
				if (projection != null && projection.SourceBuffers.Any(
					source => Utilities.IsContentOfType(source.ContentType, "HTML") || Utilities.IsContentOfType(source.ContentType, "HTMLX")))
				{
					this.textManagerLanguageName = buffer.ContentType.TypeName;
				}
			}
		}

		private void SetFileExtension(string filePath)
		{
			this.fileExtension = !string.IsNullOrEmpty(filePath) ? Path.GetExtension(filePath) : null;
			this.UpdateIsHighlightedFileExtension();
		}

		private void UpdateIsHighlightedFileExtension()
		{
			Options options = MainPackage.Instance != null ? MainPackage.Instance.Options : null;
			if (options != null && !string.IsNullOrEmpty(options.HighlightFileExtensions) && !string.IsNullOrEmpty(this.fileExtension))
			{
				string[] extensions = options.HighlightFileExtensions.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
				this.isHighlightedFileExtension = extensions.Contains(this.fileExtension, StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				this.isHighlightedFileExtension = false;
			}
		}

		private int GetTextManagerOptionValue(string valueName, int defaultValue)
		{
			int result = defaultValue;
			this.TryGetTextManagerOptionValue(valueName, ref result);
			return result;
		}

		private bool TryGetTextManagerOptionValue(string valueName, ref int optionValue)
		{
			bool result = false;

			// Note: The CacheLanguageGuids method (used by LanguageGuids.Value) depends on
			// MainPackage.Instance, so we can't pull Value until the package instance is set.
			Guid langGuid;
			if (MainPackage.Instance != null && LanguageGuids.Value.TryGetValue(this.textManagerLanguageName, out langGuid))
			{
				IVsTextManager textManager = (IVsTextManager)MainPackage.GetGlobalService(typeof(SVsTextManager));
				if (textManager != null)
				{
					LANGPREFERENCES[] langPrefs = new LANGPREFERENCES[1];
					langPrefs[0].guidLang = langGuid;
					int hr = textManager.GetUserPreferences(null, null, langPrefs, null);
					if (ErrorHandler.Succeeded(hr))
					{
						switch (valueName)
						{
							case InsertTabsOptionName:
								optionValue = unchecked((int)langPrefs[0].fInsertTabs);
								result = true;
								break;

							case TabSizeOptionName:
								optionValue = unchecked((int)langPrefs[0].uTabSize);
								result = true;
								break;
						}
					}
				}
			}

			return result;
		}

		#endregion
	}
}
