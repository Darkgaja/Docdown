﻿using Docdown.Properties;
using Docdown.Util;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Docdown.ViewModel
{
    public class SplashViewModel : ObservableObject<AppViewModel>
    {
        public static string[] Args { get; internal set; }

        private readonly Action<bool?> setDialogResult;

        public double DownloadProgress
        {
            get => downloadProgress;
            set => Set(ref downloadProgress, value);
        }

        private double downloadProgress;

        public SplashViewModel(Action<bool?> setDialogResult) : base(null)
        {
            this.setDialogResult = setDialogResult;
        }

        public void Initialize()
        {
            var settings = Settings.Default;
            var workspacePath = settings.WorkspacePath;
            var app = AppViewModel.Instance;
            bool isFile = false;
            if (Args != null && Args.Length > 0 && app.FileSystem.File.Exists(Args[0]))
            {
                isFile = true;
                if (!IOUtility.IsParent(workspacePath, Args[0]))
                {
                    workspacePath = Path.GetDirectoryName(Args[0]);
                }
            }

            Task.Run(async () =>
            {
                try
                {
                    await app.Settings.TestConnection();

                    var newVersion = await UpdateUtility.CheckNewVersion();
                    if (newVersion != null && await ShowMessageAsync("New Version",
                        $"Do you want to download Version {newVersion} now?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        IProgress<WebDownloadProgress> progress = new Progress<WebDownloadProgress>(DownloadCallback);
                        await UpdateUtility.Update(progress);
                    }

                    await app.ChangeWorkspace(workspacePath);

                    if (isFile)
                    {
                        await app.Workspace.OpenItem(Args[0]);
                    }

                    Data = app;

                    setDialogResult?.Invoke(true);
                }
                catch
                {
                    await ShowMessageAsync("Critical error", "An error occurred during startup. Application needs to close", MessageBoxButton.OK);
                    Environment.Exit(1);
                }
            });
        }

        private void DownloadCallback(WebDownloadProgress progress)
        {
            DownloadProgress = progress.Progress;
        }
    }
}
