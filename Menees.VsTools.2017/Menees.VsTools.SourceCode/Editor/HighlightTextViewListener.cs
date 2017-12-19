namespace Menees.VsTools.Editor
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel.Composition;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.Text;
	using Microsoft.VisualStudio.Text.Editor;
	using Microsoft.VisualStudio.Utilities;

	#endregion

	[Export(typeof(IWpfTextViewConnectionListener))]
	[ContentType(HighlightClassifierProvider.ContentType)]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Created by MEF.")]
	internal sealed class HighlightTextViewListener : ClassifierTextViewListenerBase
	{
		#region Constructors

		public HighlightTextViewListener()
			: base(HighlightClassifierProvider.ClassifierName)
		{
		}

		#endregion
	}
}
