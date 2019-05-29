﻿using Docdown.Model;
using Docdown.Properties;
using Docdown.ViewModel;
using MahApps.Metro;
using System;
using System.Windows;

namespace Docdown
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.AddAccent("BlueDoc", GetResourceUri("Resources/Accents/Blue.xaml"));
            ThemeManager.AddAppTheme("DarkDoc", GetResourceUri("Resources/Themes/Dark.xaml"));
            ThemeManager.AddAppTheme("LightDoc", GetResourceUri("Resources/Themes/Light.xaml"));
            var theme = Settings.Default.Theme;
            ChangeLocale(Settings.Default.Locale);
            ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent("BlueDoc"), ThemeManager.GetAppTheme(theme + "Doc"));
            SplashViewModel.Args = e.Args;
            base.OnStartup(e);
        }

        public static Uri GetResourceUri(string resource)
        {
            return new Uri("pack://application:,,,/Docdown;component/" + resource);
        }

        public static void ReloadIcons()
        {
            LoadResource(GetResourceUri("Resources/Images/Icons.xaml"));
        }

        public static void ChangeLocale(string locale)
        {
            var language = Language.Load(locale);
            SetResources(language.Dictionary);
        }

        private static void LoadResource(Uri uri)
        {
            var resourceDictionary = new ResourceDictionary
            {
                Source = uri
            };
            SetResources(resourceDictionary);
        }

        private static void SetResources(ResourceDictionary dictionary)
        {
            Current.Resources.MergedDictionaries.Remove(dictionary);
            Current.Resources.MergedDictionaries.Add(dictionary);
        }
    }
}