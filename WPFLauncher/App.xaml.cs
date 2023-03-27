﻿using System.Threading;
using System.Windows;

namespace WPFLauncher
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex _instanceMutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            // check that there is only one instance of the control panel running...
            bool createdNew;
            _instanceMutex = new Mutex(true, "OpenDAoCLauncher" , out createdNew);
            if (!createdNew)
            {
                _instanceMutex = null;
                
                MessageBox.Show("OpenDAoC Launcher is already running!", "", MessageBoxButton.OK);
                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_instanceMutex != null)
                _instanceMutex.ReleaseMutex();
            base.OnExit(e);
        }
    }
}