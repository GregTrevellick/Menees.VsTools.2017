namespace Menees.VsTools.Tasks
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
	using Microsoft.VisualStudio.Shell;

	#endregion

	[Guid(Guids.TasksWindowPersistanceString)]
	internal sealed class TasksWindow : ToolWindowPane
	{
		#region Constructors

		public TasksWindow()
			: base(null)
		{
			// Set the window title reading it from the resources.
			this.Caption = Properties.Resources.TasksWindowTitle;

			// Set the image that will appear on the tab of the window frame
			// when docked with an other window
			// The resource ID correspond to the one defined in the resx file
			// while the Index is the offset in the bitmap strip. Each image in
			// the strip being 16x16.
			const int ResourceId = 301;
			this.BitmapResourceID = ResourceId; // In VSPackage.resx, the Images.png file is an image resource named "301".
			const int ImageIndex = 19;
			this.BitmapIndex = ImageIndex; // This is a 1-based image index.  From <GuidSymbol name="Images"> in Menees.VsTools.vsct.

			// This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
			// we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
			// the object returned by the Content property.
			this.Content = new TasksControl(this);
		}

		#endregion
	}
}
