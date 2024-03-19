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
            // Process command line args
            var isAutoStart = false;
            //MessageBox.Show("hi");
            for (int i = 0; i != e.Args.Length; ++i)
            {
               
                if (e.Args[i] == "/autoStartupAutoRefreshRate")
                {
                   
                    isAutoStart = true;
                }
            }
            // Create main application window, starting minimized if specified
            MainWindow mainWindow = new MainWindow();

            //  MessageBox.Show("check auto start: " + isAutoStart.ToString());
            if (isAutoStart)
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
            mainWindow.OnAutoStart();
        }
    
            // Application is running
            // Process command line args
            //    bool startMinimized = false;
            //    for (int i = 0; i != e.Args.Length; ++i)
            //    {
            //        if (e.Args[i] == "/StartMinimized")
            //        {
            //            startMinimized = true;
            //        }
            //    }

            //    // Create main application window, starting minimized if specified
            //    MainWindow mainWindow = new MainWindow();
            //    if (startMinimized)
            //    {
            //        mainWindow.WindowState = WindowState.Minimized;
            //    }

            //mainWindow.Show();
            ////base.OnStartup(e);

       // }

       
    }

}
