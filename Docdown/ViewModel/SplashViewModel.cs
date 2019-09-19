﻿using Docdown.Model;
using Docdown.Properties;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using Docdown.Windows;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docdown.ViewModel
{
    public class SplashViewModel : ObservableObject<AppViewModel>
    {
        public static string[] Args { get; internal set; }

        private readonly Action<bool?> setDialogResult;

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
                await app.Settings.TestConnection();
                await app.ChangeWorkspace(workspacePath);

                if (isFile)
                {
                    app.Workspace.OpenItem(Args[0]);
                }

                Data = app;

                setDialogResult?.Invoke(true);
            });
        }
    }
}
