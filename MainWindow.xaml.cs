using System.Diagnostics;
using System.Windows;


namespace AutoRefreshRate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly NotifyIcon _notifyIcon;
        public MainWindow()
        {
          //  DataContext = new SettingsViewVM();
            InitializeComponent();
            Closing += onPreviewClosed;

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = new System.Drawing.Icon("Resources/Icons/logo.ico");
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Auto Refresh Rate";

            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();


           // ToolStripMenuItem tsBtnEnable = new ToolStripMenuItem("Enable");
           //SettingsViewVM vm = new SettingsViewVM();
           // this.DataContext = vm;
           //Binding ts = new Binding("Checked", vm, null);
           // tsBtnEnable.DataBindings.Add(ts);
           // //  tsBtnEnable.Checked
           // tsBtnEnable.CheckOnClick = true;

            ToolStripMenuItem btnExit = new ToolStripMenuItem("Exit", Image.FromFile("Resources/Icons/exit.ico"), ExitButton_Click);

            ToolStripDropDownButton toolStripDropDown = new ToolStripDropDownButton();
            toolStripDropDown.Text = "Help";
            toolStripDropDown.AutoToolTip = false;

            ToolStripMenuItem btnHelp = new ToolStripMenuItem("Help", Image.FromFile("Resources/Icons/help.ico"));
            ToolStripMenuItem btnGithub = new ToolStripMenuItem("Github", Image.FromFile("Resources/Icons/github.ico"));



            btnGithub.AutoToolTip = false;
            btnGithub.ForeColor = System.Drawing.Color.Blue;
            btnGithub.Click += Button_Click;

            btnHelp.DropDownItems.Add(btnGithub);



            //_notifyIcon.ContextMenuStrip.Items.Add(tsBtnEnable);
            _notifyIcon.ContextMenuStrip.Items.Add(btnHelp); ;
            _notifyIcon.ContextMenuStrip.Items.Add(btnExit);
            _notifyIcon.DoubleClick += toogleApp;
        }

      
        private void ExitButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private void Button_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/ngtuonghy"; 
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        private void toogleApp(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }
        private void onPreviewClosed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();

        }
        public void OnAutoStart()
        {
            if (WindowState == WindowState.Minimized)
            {

                WindowState = WindowState.Normal;

                Hide();
           }
            else
            {
                Show();
            }
        }
    }
}