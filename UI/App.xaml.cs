using GitViz.Logic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var vm = new ViewModel();

            if (e.Args.Length == 1)
                vm.RepositoryPath = e.Args[0];
            var wnd = new MainWindow();
            wnd.DataContext = vm;
            wnd.Show();
        }

    }
}
