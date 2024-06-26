﻿using System;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Task = System.Threading.Tasks.Task;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.IO;

[assembly: InternalsVisibleTo("WebFormsMappedHotReloadTests")]
namespace WebFormsMappedHotReload
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideService(typeof(WebFormsMappedHotReloadPackage), IsAsyncQueryable = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(OptionPageGrid), "WebForms Mapped Hot Reload", "General", 0, 0, true)]
    public sealed class WebFormsMappedHotReloadPackage : AsyncPackage
    {
        /// <summary>
        /// WebFormsMappedHotReloadPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "9A64C9DC-24CD-4C65-B1BD-6B9F3EAF2734";

        private DTE _dte;
        private DocumentEvents _dteDocumentEvents;
        private Helper _helper;

        #region Package Members

        /// <summary>
        /// Initialisation of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the Initialisation code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for Initialisation cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package Initialisation, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            // When Initialised asynchronously, the current thread may be a background thread at this point.
            // Do any Initialisation that requires the UI thread after switching to the UI thread.
            //await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _helper = new Helper();

            GetLogger().LogInformation(GetPackageName(), "Initialising...");
            await base.InitializeAsync(cancellationToken, progress);

            try
            {
                await BindToLocalVisualStudioEventsAsync();

                GetLogger().LogInformation(GetPackageName(), "Initialised.");
            }
            catch (Exception exception)
            {
                GetLogger().LogError(GetPackageName(), "Exception during initialisation", exception);
            }
        }

        private async Task BindToLocalVisualStudioEventsAsync()
        {
            _dte = (DTE)await GetServiceAsync(typeof(DTE));
            var _dteEvents = _dte.Events;

            _dteDocumentEvents = _dteEvents.DocumentEvents;

            _dteDocumentEvents.DocumentSaved += OnDocumentSaved;
        }

        private void OnDocumentSaved(Document document)
        {
            //TODO: determine debugger is attached
            if (true)
            {
                bool isRazor = false;
                if (document?.Language == "HTML")
                {
                    if (Path.GetExtension((document != null) ? document.Name : null).ToLower() == ".cshtml") isRazor = true;
                    if (Path.GetExtension((document != null) ? document.Name : null).ToLower() == ".vbhtml") isRazor = true;
                }

                if (document?.Saved == true && (document?.Language == "WebForms" || isRazor))
                {
                    var filepath = document.FullName;
                    string rootPath = System.IO.Path.GetPathRoot(filepath);
                    System.IO.DriveInfo driveInfo = new System.IO.DriveInfo(rootPath);
                    if (driveInfo.DriveType == DriveType.Network)
                    {
                        var currentBytesArray = System.IO.File.ReadAllBytes(filepath);
                        System.IO.File.WriteAllBytes(filepath, currentBytesArray);
                    }
                }
            }
        }

        private string GetPackageName() => nameof(WebFormsMappedHotReloadPackage);

        private IVsActivityLog GetLogger()
        {
            return this.GetService(typeof(SVsActivityLog)) as IVsActivityLog ?? new NullLogger();
        }
        #endregion
    }
}
