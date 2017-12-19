namespace Menees.VsTools.Editor
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.ComponentModel.Design;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Drawing.Design;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web.UI.WebControls;

	#endregion

	internal sealed class OutputHighlight
	{
		#region Private Data Members

		private const string DefaultPattern = ".*";
		private const OutputHighlightType DefaultHighlight = OutputHighlightType.Detail;

		private string name;
		private string[] contentTypes;
		private string pattern;

		#endregion

		#region Constructors

		public OutputHighlight()
			: this("All Text", DefaultHighlight, DefaultPattern)
		{
		}

		internal OutputHighlight(
			string name,
			OutputHighlightType highlight,
			string pattern,
			string[] contentTypes = null)
		{
			this.Name = name;
			this.Highlight = highlight;
			this.Pattern = pattern;
			this.ContentTypes = contentTypes ?? new[] { OutputClassifierProvider.ContentType };
		}

		#endregion

		#region Public Properties

		[Category("Display")]
		[DefaultValue(null)]
		[DisplayName("Name")]
		public string Name
		{
			get
			{
				return this.name;
			}

			set
			{
				string trimmedValue = (value ?? string.Empty).Trim();
				if (this.name != trimmedValue)
				{
					if (string.IsNullOrEmpty(trimmedValue))
					{
						throw new ArgumentException("The name cannot be empty or all whitespace.", "value");
					}

					this.name = trimmedValue;
				}
			}
		}

		[Category("Display")]
		[DefaultValue(DefaultHighlight)]
		[DisplayName("Highlight as")]
		public OutputHighlightType Highlight { get; set; }

		[Category("Display")]
		[DisplayName("Content types")]
		[TypeConverter(typeof(StringArrayConverter))]
		public string[] ContentTypes
		{
			get
			{
				return this.contentTypes;
			}

			set
			{
				if (this.contentTypes != value)
				{
					// Parse first to make sure it's valid before we update the displayed member value.
					ValidateContentTypes(value);
					this.contentTypes = value;
				}
			}
		}

		[Category("Pattern")]
		[DefaultValue(DefaultPattern)]
		[DisplayName("Regular expression to match")]
		public string Pattern
		{
			get
			{
				return this.pattern;
			}

			set
			{
				if (this.pattern != value)
				{
					// We won't create a Regex instance here because MatchCase may change later.
					// We'll use IsMatch to validate the pattern, but we also won't allow an empty
					// pattern.  It could only ever match empty lines, which we can't highlight.
					Regex.IsMatch(string.Empty, value);
					if (string.IsNullOrEmpty(value))
					{
						throw new ArgumentException("An empty pattern cannot be used to highlight output lines.");
					}

					this.pattern = value;
				}
			}
		}

		[Category("Pattern")]
		[DefaultValue(false)]
		[DisplayName("Match case")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Set by PropertyGrid")]
		public bool MatchCase { get; set; }

		#endregion

		#region Public Methods

		public static void ValidateContentTypes(string[] contentTypes)
		{
			HashSet<string> valid = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

			// Microsoft's standard Output-derived content types are listed in OutputClassifierProvider's
			// KnownContentTypes.  However, other custom content types can be registered.
			HashSet<string> knownOutputContentTypes = new HashSet<string>(
				OutputClassifierProvider.GetOutputContentTypes(),
				StringComparer.CurrentCultureIgnoreCase);

			if (contentTypes != null)
			{
				foreach (string contentType in contentTypes
					.Where(s => !string.IsNullOrEmpty(s))
					.Select(s => s.Trim())
					.Where(s => s.Length > 0))
				{
					// Only add types that are known to be valid (or add them all if there are no known types).
					if (knownOutputContentTypes.Count == 0 || knownOutputContentTypes.Contains(contentType))
					{
						valid.Add(contentType);
					}
				}
			}

			if (valid.Count == 0)
			{
				throw new ArgumentException("You must specify one of the registered Output content types: " +
					string.Join(", ", knownOutputContentTypes.OrderBy(s => s)));
			}
		}

		public override string ToString()
		{
			string result = this.Name;

			if (string.IsNullOrEmpty(result))
			{
				result = this.Highlight.ToString();
				if (!string.IsNullOrEmpty(this.Pattern))
				{
					result += ": " + this.Pattern;
				}
			}

			return result;
		}

		#endregion
	}
}
