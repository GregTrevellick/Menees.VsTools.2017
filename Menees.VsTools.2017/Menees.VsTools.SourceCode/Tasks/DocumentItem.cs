namespace Menees.VsTools.Tasks
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.Editor;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;
	using Microsoft.VisualStudio.Text;
	using Microsoft.VisualStudio.TextManager.Interop;
	using Microsoft.VisualStudio.Utilities;

	#endregion

	internal sealed class DocumentItem
	{
		#region Private Data Members

		private ITextDocument textDocument;

		#endregion

		#region Constructors

		public DocumentItem(ITextDocument textDocument)
		{
			// Note: textDocument can be null in some situations (e.g., if we know a miscellaneous
			// file has an document tab visible but the document hasn't been initialized in the running
			// document table yet so no ITextDocument has been created).
			this.textDocument = textDocument;
		}

		#endregion

		#region Public Properties

		public Language Language
		{
			get
			{
				Language result = Language.Unknown;

				if (this.textDocument != null)
				{
					IContentType contentType = this.textDocument.TextBuffer.ContentType;
					string fileName = this.textDocument.FilePath;
					result = GetDocumentLanguage(contentType, fileName);
				}

				return result;
			}
		}

		public bool HasTextDocument
		{
			get
			{
				bool result = this.textDocument != null;
				return result;
			}
		}

		public DateTime? LastModifiedUtc
		{
			get
			{
				DateTime? result = null;

				if (this.textDocument != null)
				{
					result = this.textDocument.LastContentModifiedTime.ToUniversalTime();
				}

				return result;
			}
		}

		#endregion

		#region Public Methods

		public static Language GetLanguage(ITextBuffer buffer)
		{
			ITextDocument doc = GetTextDocument(buffer);
			string fileName = doc != null ? doc.FilePath : null;
			Language result = GetDocumentLanguage(buffer.ContentType, fileName);
			return result;
		}

		public static ITextDocument GetTextDocument(ITextBuffer buffer)
		{
			// https://social.msdn.microsoft.com/Forums/vstudio/en-US/0f6ef03a-df6b-4670-856e-f4a539fbfbe1/how-get-document-name-of-an-iwpftextview
			ITextDocument result = null;

			if (buffer != null)
			{
				ITextDocument document;
				if (buffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out document))
				{
					result = document;
				}
			}

			return result;
		}

		public static ITextDocument GetTextDocument(IVsWindowFrame frame, IVsEditorAdaptersFactoryService adapterFactory)
		{
			ITextDocument result = null;

			// http://stackoverflow.com/a/7373385/1882616
			IVsTextView view = VsShellUtilities.GetTextView(frame);
			if (view != null)
			{
				IVsTextLines lines;
				if (view.GetBuffer(out lines) == 0)
				{
					var oldVsBuffer = lines as IVsTextBuffer;
					if (oldVsBuffer != null)
					{
						ITextBuffer buffer = adapterFactory.GetDataBuffer(oldVsBuffer);
						result = GetTextDocument(buffer);
					}
				}
			}

			return result;
		}

		public IEnumerable<string> GetLines()
		{
			if (this.textDocument != null)
			{
				ITextSnapshot snapshot = this.textDocument.TextBuffer.CurrentSnapshot;
				foreach (ITextSnapshotLine line in snapshot.Lines)
				{
					yield return line.GetText();
				}
			}
		}

		#endregion

		#region Private Methods

		private static Language GetDocumentLanguage(IContentType contentType, string fileName)
		{
			Language result = Utilities.GetLanguage(contentType.TypeName, fileName);
			if (result == Language.Unknown)
			{
				foreach (IContentType baseType in contentType.BaseTypes)
				{
					result = GetDocumentLanguage(baseType, fileName);
					if (result != Language.Unknown)
					{
						break;
					}
				}
			}

			return result;
		}

		#endregion
	}
}
