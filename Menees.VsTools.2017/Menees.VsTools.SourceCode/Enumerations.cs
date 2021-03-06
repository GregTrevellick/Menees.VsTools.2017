﻿namespace Menees.VsTools
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	#endregion

	#region GuidFormat

	/// <summary>
	/// The supported formats for <see cref="Guid.ToString(string)"/>.
	/// </summary>
	public enum GuidFormat
	{
		/// <summary>
		/// 32 digits separated by hyphens:
		/// 00000000-0000-0000-0000-000000000000
		/// </summary>
		Dashes,

		/// <summary>
		/// 32 digits:
		/// 00000000000000000000000000000000
		/// </summary>
		Numbers,

		/// <summary>
		/// 32 digits separated by hyphens, enclosed in braces:
		/// {00000000-0000-0000-0000-000000000000}
		/// </summary>
		Braces,

		/// <summary>
		/// 32 digits separated by hyphens, enclosed in parentheses:
		/// (00000000-0000-0000-0000-000000000000)
		/// </summary>
		Parentheses,

		/// <summary>
		/// Four hexadecimal values enclosed in braces, where the fourth value is a subset of eight hexadecimal values that is also enclosed in braces:
		/// {0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}}
		/// </summary>
		Structure,
	}

	#endregion

	#region BuildTiming

	internal enum BuildTiming
	{
		None,
		Details,
		Overall,
	}

	#endregion

	#region Command

	internal enum Command
	{
		// Note: These values must match the IDSymbol values used in the VSCT file.
		SortLines = 301,
		Trim = 302,
		Statistics = 303,
		StreamText = 304,
		CheckSpelling = 305,
		GenerateGuid = 306,
		ExecuteText = 307,
		ExecuteFile = 308,
		ToggleFiles = 309,
		ToggleReadOnly = 310,
		AddRegion = 311,
		CollapseAllRegions = 312,
		ExpandAllRegions = 313,
		ViewBaseConverter = 314,
		ListAllProjectProperties = 315,
		CommentSelection = 316,
		UncommentSelection = 317,
		SortMembers = 318,
		AddToDoComment = 319,
		ViewTasks = 320,
	}

	#endregion

	#region Language

	internal enum Language
	{
		Unknown,
		CPlusPlus,
		HTML,
		XML,
		VB,
		CSharp,
		CSS,
		PlainText,
		SQL,
		IDL,
		VBScript,
		JavaScript,
		FSharp,
		TypeScript,
		PowerShell,
		Python

		// Note: When you add a language here, also add it to Tasks\ScanInfo.xml
		// (e.g., as a new <Languages> node under an existing <Style>).
	}

	#endregion

	#region MemberAccess

	internal enum MemberAccess
	{
		// These member kinds are in order by StyleCop's preferences (SA1202):
		Unknown,
		Public,
		Internal,
		ProtectedOrInternal,
		Protected,
		Private,
		Default,
		AssemblyOrFamily,
		WithEvents,
	}

	#endregion

	#region MemberKind

	internal enum MemberKind
	{
		// These member kinds are in order by StyleCop's preferences (SA1201):
		// http://stackoverflow.com/questions/150479/order-of-items-in-classes-fields-properties-constructors-methods/310967#310967
		// http://www.stylecop.com/docs/Ordering%20Rules.html
		Unknown,
		Variable,
		Constructor,
		Destructor,
		Event,
		Property,
		Function,
		Operator,
	}

	#endregion

	#region MemberProperty

	internal enum MemberProperty
	{
		// Note: These enum field names are used in the Description attribute for Options.SortMembersOrder.
		Kind,
		Name,
		IsStatic,
		Access,
		ParameterCount,
		OverrideModifier,
		ConstModifier,
		KindModifier,
	}

	#endregion

	#region NumberBase

	internal enum NumberBase
	{
		Binary = 2,
		Decimal = 10,
		Hex = 16
	}

	#endregion

	#region NumberByteOrder

	// Note: These must match the order of the items in the byteOrder combo box.
	internal enum NumberByteOrder
	{
		Numeric = 0,
		LittleEndian = 1,
		BigEndian = 2

		// Note: The only important difference between Numeric and BigEndian is
		// how BaseConverter.ValidateAndNormalizeByteText zero pads the values.
		// For Numeric, the values are always left-padded.  For BigEndian, a partial
		// byte is left-padded, then the rest is right-padded.
	}

	#endregion

	#region NumberType

	// Note: These must match the order of the items in the dataType combo box.
	internal enum NumberType
	{
		SByte,
		Byte,
		Int16,
		UInt16,
		Int32,
		UInt32,
		Int64,
		UInt64,
		Single,
		Double,
		Decimal,
	}

	#endregion
}
