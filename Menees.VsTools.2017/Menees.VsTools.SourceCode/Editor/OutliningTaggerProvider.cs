namespace Menees.VsTools.Editor
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using Menees.VsTools.Tasks;
	using Microsoft.VisualStudio.Text;
	using Microsoft.VisualStudio.Text.Tagging;
	using Microsoft.VisualStudio.Utilities;

	#endregion

	// Note: The PowerShell and Python providers already support #regions, so we can't trump them.
	[Export(typeof(ITaggerProvider))]
	[TagType(typeof(IOutliningRegionTag))]
	[ContentType("html")]
	[ContentType("htmlx")]
	[ContentType("javascript")]
	[ContentType("node.js")]
	[ContentType("SQL Server Tools")]
	[ContentType("T-SQL90")]
	[ContentType("TypeScript")]
	[ContentType("XAML")]
	[ContentType("XML")]
	[ContentType("XOML")]
	internal sealed class OutliningTaggerProvider : ITaggerProvider
	{
		#region Public Methods

		public ITagger<T> CreateTagger<T>(ITextBuffer buffer)
			where T : ITag
		{
			Func<OutliningTagger> createBufferTagger = () =>
				{
					Language language = DocumentItem.GetLanguage(buffer);
					ScanInfo scanInfo;
					ScanInfo.TryGet(language, out scanInfo);
					return new OutliningTagger(buffer, scanInfo);
				};

			OutliningTagger result = buffer.Properties.GetOrCreateSingletonProperty<OutliningTagger>(createBufferTagger);
			return result as ITagger<T>;
		}

		#endregion
	}
}
