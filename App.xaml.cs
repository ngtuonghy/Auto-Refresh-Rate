using System.Windows;
using Application = System.Windows.Application;


namespace AutoRefreshRate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            var isAutoStart = false;
            for (int i = 0; i != e.Args.Length; ++i)
            {
               
                if (e.Args[i] == "/autoStartupAutoRefreshRate")
                {
                   
                    isAutoStart = true;
                }
            }
            MainWindow mainWindow = new MainWindow();
            if (isAutoStart)
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
            mainWindow.OnAutoStart();
        }
    }

}
