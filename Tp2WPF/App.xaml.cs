using System.Configuration;
using System.Data;
using System.Windows;

namespace Tp2WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string[] Args { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Args = e.Args; 

            var mainWindow = new MainWindow();
        }
    }

}
