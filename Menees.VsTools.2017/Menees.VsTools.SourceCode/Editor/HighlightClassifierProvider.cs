namespace Menees.VsTools.Editor
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.Text;
	using Microsoft.VisualStudio.Text.Classification;
	using Microsoft.VisualStudio.Text.Editor;
	using Microsoft.VisualStudio.Utilities;

	#endregion

	/// <summary>
	/// This class causes a <see cref="HighlightClassifier"/> to be added to the set of classifiers.
	/// Since the content type is set to "text", this classifier applies to all text files.
	/// </summary>
	[Export(typeof(IClassifierProvider))]
	[ContentType(HighlightClassifierProvider.ContentType)]
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Created by MEF.")]
	internal sealed class HighlightClassifierProvider : ClassifierProviderBase
	{
		#region Internal Constants

		internal const string ContentType = "text";
		internal const string ClassifierName = "Editor Highlight";

		#endregion

		#region Constructors

		public HighlightClassifierProvider()
			: base(ClassifierName)
		{
		}

		#endregion

		#region Internal Properties

		/// <summary>
		/// Import the service so we can get the text document for a text buffer.
		/// </summary>
		[Import]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by MEF.")]
		internal ITextDocumentFactoryService ITextDocumentFactory { get; set; }

		#endregion

		#region Protected Methods

		protected override ClassifierBase CreateClassifier(ITextBuffer buffer)
		{
			// We only have one classification type for editor highlighting, so we'll get it and pass it in from here.
			IClassificationType classificationType = this.ClassificationRegistry.GetClassificationType(HighlightFormat.ClassificationName);
			IEditorOptions editorOptions = this.EditorOptionsFactory.GetOptions(buffer);
			HighlightClassifier result = new HighlightClassifier(buffer, classificationType, editorOptions, this.ITextDocumentFactory);
			return result;
		}

		#endregion
	}
}
