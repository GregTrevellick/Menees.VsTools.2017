[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Microsoft.Design",
	"CA1020:AvoidNamespacesWithFewTypes",
	Scope = "namespace",
	Target = "Menees.VsTools",
	Justification = "VS extension assemblies only need to expose one public type.")]

namespace Menees.VsTools
{
	#region Using Directives

	using System;
	using System.ComponentModel.Design;
	using System.Diagnostics;
	using System.Globalization;
	using System.Runtime.InteropServices;
	using Menees.VsTools.Editor;
	using Menees.VsTools.Tasks;
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.OLE.Interop;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;
	using Microsoft.Win32;

	#endregion

	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the
	/// IVsPackage interface and uses the registration attributes defined in the framework to
	/// register itself and its components with the shell.
	/// </summary>
	[PackageRegistration(UseManagedResourcesOnly = true)] // Tells the PkgDef creation utility (CreatePkgDef.exe) that this class is a package.
	[InstalledProductRegistration("#110", "#111", MainPackage.Version, IconResourceID = 400)] // Registers the information needed to show this package in the
	// Help/About dialog of Visual Studio.
	[ProvideMenuResource("Menus.ctmenu", 1)] // This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideToolWindow(typeof(BaseConverterWindow), Style = VsDockStyle.Tabbed, Window = ToolWindowGuids.PropertyBrowser)] // Registers a tool window.
	[ProvideToolWindow(typeof(TasksWindow), Style = VsDockStyle.Tabbed, Window = ToolWindowGuids.SolutionExplorer)] // Registers a tool window.
	[ProvideOptionPage(
		typeof(Options),
		categoryName: MainPackage.Title,
		pageName: "General",
		categoryResourceID: 113,
		pageNameResourceID: 112,
		supportsAutomation: false,
		SupportsProfiles = true,
		ProfileMigrationType = ProfileMigrationType.PassThrough)] // Registers an Options page
	[ProvideProfile(
		typeof(Options),
		categoryName: MainPackage.Title,
		objectName: MainPackage.Title,
		categoryResourceID: 113,
		objectNameResourceID: 113,
		isToolsOptionPage: true,
		DescriptionResourceID = 114,
		MigrationType = ProfileMigrationType.PassThrough)] // Registers settings persistence.
		// Affects Import/Export Settings.
	[ProvideAutoLoad(VSConstants.UICONTEXT.CodeWindow_string)] // See comments in Menees.VsTools.vsct
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.EmptySolution_string)] // Note: UICONTEXT.CodeWindow_string doesn't work as I expected it to in VS 2012, so...
	[ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)] // ...I also added Empty and NoSolution contexts to get the package to load correctly.  :-(
	[Guid(Guids.MeneesVsToolsPackageString)]
	[CLSCompliant(false)]
	public sealed partial class MainPackage : Package, IDisposable
	{
		#region Private Data Members

		private CommandProcessor processor;
		private Options options;
		private ClassificationFormatManager formatManager;
		private CommentTaskProvider commentTaskProvider;
		private BuildTimer buildTimer;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require
		/// any Visual Studio service because at this point the package object is created but
		/// not sited yet inside Visual Studio environment. The place to do all the other
		/// initialization is the Initialize method.
		/// </summary>
		public MainPackage()
		{
			LogMessage(string.Format("Entering {0} constructor", this.ToString()));
		}

		#endregion

		#region Internal Properties

		internal static MainPackage Instance { get; private set; }

		internal Options Options
		{
			get
			{
				// Ryan Molden from Microsoft says that GetDialogPage caches the result,
				// so we're going to cache it too and make it into a strongly-typed property.
				// I've also verified GetDialogPage's caching implementation in VS11 by looking
				// at it with Reflector.
				// http://social.msdn.microsoft.com/Forums/eu/vsx/thread/303fce01-dfc0-43b3-a578-8b3258c0b83f
				//
				// Note: This is also important because the Base Converter control will attach
				// to change notification events on this cached object instance.
				if (this.options == null)
				{
					// From http://msdn.microsoft.com/en-us/library/bb165039.aspx
					this.options = this.GetDialogPage(typeof(Options)) as Options;
				}

				return this.options;
			}
		}

		internal CommentTaskProvider TaskProvider => this.commentTaskProvider;

		internal System.IServiceProvider ServiceProvider
		{
			get
			{
				// This convoluted code forces a dynamic cast rather than using a simple static cast because starting with
				// VS 2017 Update 3, Microsoft added a package interface that's not in any public assembly.  That causes
				// a compile-time error if a static cast is used on "this" to directly get an IServiceProvider:
				// error CS1748: Cannot find the interop type that matches the embedded interop type
				//    'Microsoft.VisualStudio.Shell.Interop.IVsToolboxItemProvider2'. Are you missing an assembly reference?
				object temp = this;
				var result = temp as System.IServiceProvider;
				return result;
			}
		}

		#endregion

		#region Public Methods

		public void Dispose()
		{
			base.Dispose(true);
		}

		#endregion

		#region Internal Methods

		[Conditional("TRACE")]
		internal static void LogException(Exception ex)
		{
			LogMessage(null, ex);
		}

		[Conditional("TRACE")]
		internal static void LogMessage(string message, Exception ex = null)
		{
			if (ex != null)
			{
				string activityLogMessage = string.IsNullOrEmpty(message) ? ex.ToString() : message + "\r\n" + ex.ToString();
				ActivityLog.LogError(typeof(MainPackage).FullName, activityLogMessage);
				Log.Error(typeof(MainPackage), message, ex);
			}
			else
			{
				ActivityLog.LogInformation(typeof(MainPackage).FullName, message);
				Log.Info(typeof(MainPackage), message, ex);
			}
		}

		internal void ShowMessageBox(string message, bool isError = false)
		{
			IVsUIShell uiShell = (IVsUIShell)this.GetService(typeof(SVsUIShell));
			Guid clsid = Guid.Empty;

			// This has a pszTitle parameter, but it isn't used for the MessageBox's title.
			// VS just appends the pszTitle to the front of the pszText and separates it
			// with a couple of newlines.
			ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
				0,
				ref clsid,
				string.Empty,
				message,
				string.Empty,
				0,
				OLEMSGBUTTON.OLEMSGBUTTON_OK,
				OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
				isError ? OLEMSGICON.OLEMSGICON_CRITICAL : OLEMSGICON.OLEMSGICON_INFO,
				0,        // false
				out int result));
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			LogMessage(string.Format("Entering {0}.Initialize()", this.ToString()));
			try
			{
				base.Initialize();
				LogMessage(string.Format("After {0}'s base.Initialize()", this.ToString()));

				// Set a static instance, so anything later (e.g., editor extensions) can get to this package and its options.
				Instance = this;

				this.processor = new CommandProcessor(this);

				// Add our command handlers.  Commands must exist in the .vsct file.
				if (this.GetService(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
				{
					foreach (Command id in Enum.GetValues(typeof(Command)))
					{
						CommandID commandId = new CommandID(Guids.MeneesVsToolsCommandSet, (int)id);

						// OleMenuCommand extends the base MenuCommand to add BeforeQueryStatus.
						// http://msdn.microsoft.com/en-us/library/bb165468.aspx
						OleMenuCommand menuItem = new OleMenuCommand(this.Command_Execute, commandId);
						menuItem.BeforeQueryStatus += this.Command_QueryStatus;
						mcs.AddCommand(menuItem);
					}
				}

				// To make our font and color formats customizable in non-TextEditor windows,
				// (e.g., Output) we have to hook some old-school TextManager COM events.
				this.formatManager = new ClassificationFormatManager(this.ServiceProvider);
				this.formatManager.UpdateFormats();

				// This option requires a restart if changed because the CommentTaskProvider and various
				// XxxMonitor classes attach to too many events and register too many things to easily
				// detach/unregister and clean them all up if this is toggled interactively.
				if (this.Options.EnableCommentScans)
				{
					this.commentTaskProvider = new CommentTaskProvider(this);
				}

				this.buildTimer = new BuildTimer(this);
			}
			catch (Exception ex)
			{
				LogMessage(string.Format("An unhandled exception occurred in {0}.Initialize().", this.ToString()), ex);
				throw;
			}

			LogMessage(string.Format("Exiting {0}.Initialize()", this.ToString()));
		}

		protected override void Dispose(bool disposing)
		{
			if (this.commentTaskProvider != null)
			{
				this.commentTaskProvider.Dispose();
			}

			if (this.buildTimer != null)
			{
				this.buildTimer.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Private Event Handlers

		private void Command_Execute(object sender, EventArgs e)
		{
			if (sender is OleMenuCommand menuCommand)
			{
				Command command = (Command)menuCommand.CommandID.ID;
				this.processor.Execute(command);
			}
		}

		private void Command_QueryStatus(object sender, EventArgs e)
		{
			if (sender is OleMenuCommand menuCommand)
			{
				Command command = (Command)menuCommand.CommandID.ID;
				bool canExecute = this.processor.CanExecute(command);
				menuCommand.Enabled = canExecute;
				menuCommand.Visible = canExecute;
			}
		}

		#endregion
	}
}
