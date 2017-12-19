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
	using System.Windows.Media;
	using Microsoft.VisualStudio.Text.Classification;
	using Microsoft.VisualStudio.Utilities;

	#endregion

	/// <summary>
	/// Defines an editor format for the <see cref="HighlightType"/>.
	/// </summary>
	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = ClassificationName)]
	[Name(ClassificationName)]
	[UserVisible(true)]
	[Order(Before = Priority.Default)] // Highlight is higher precendence than syntax highlighting, which is important if comments use a background color.
	internal sealed class HighlightFormat : ClassificationFormatBase
	{
		#region Public Constants

		public const string ClassificationName = ClassificationFormatBase.ClassificationBasePrefix + HighlightClassifierProvider.ClassifierName;

		private static readonly Color LightYellow = Color.FromRgb(255, 255, 110); // A lighter yellow than Colors.Yellow.

		#endregion

		#region Constructors

		/// <summary>
		/// Defines the default visual format for the <see cref="HighlightType"/> classification type.
		/// </summary>
		public HighlightFormat()
			: base(ClassificationName)
		{
			// If we're highlighting whitespace, then the foreground color will be useless, so we need a background color.
			this.BackgroundColor = LightYellow;
		}

		#endregion

		#region Internal Properties

		/// <summary>
		/// Defines the ClassificationType for highlighting.
		/// </summary>
		[Export(typeof(ClassificationTypeDefinition))]
		[Name(ClassificationName)]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by MEF.")]
		internal static ClassificationTypeDefinition HighlightType { get; set; }

		#endregion
	}
}
