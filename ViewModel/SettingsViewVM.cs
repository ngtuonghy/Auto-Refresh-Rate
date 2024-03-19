using AutoRefreshRate.Models;
using AutoRefreshRate.Util;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using IWshRuntimeLibrary;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
#nullable disable

namespace AutoRefreshRate.ViewModel
{
    public class SettingsViewVM : Util.ViewModelBase
    {
        readonly string _appPath = Directory.GetCurrentDirectory();
        readonly Util.FileHandler _fileHandler;
        private DisplaySpecs _displaySpecs;
        public static int _previousBatteryState = 0; // 1 is battery | 2 is plugin |  0 reset check
                                                   
        public enum ChargingStatus
        {
            Charging,       // Đang sạc
            Discharging,    // Đang xả pin
            Unknown         // Trạng thái không xác định
        }

        public string refreshRate
        {
            get { return _displaySpecs.refreshRate; }
            set
            {
                if (_displaySpecs.refreshRate != value)
                {
                    _displaySpecs.refreshRate = value;
                    OnPropertyChanged("refreshRate");
                }
            }
        }

        private ObservableCollection<ComboBoxItem> _comboBoxItemsBattery;
        public ObservableCollection<ComboBoxItem> comboBoxItemsBattery
        {
            get { return _comboBoxItemsBattery; }
            set
            {
                if (_comboBoxItemsBattery != value)
                {

                    _comboBoxItemsBattery = value;
                    OnPropertyChanged("comboBoxItemsBattery");

                }
            }
        }

        private ObservableCollection<ComboBoxItem> _comboBoxItemsPlugged;
        public ObservableCollection<ComboBoxItem> comboBoxItemsPlugged
        {
            get { return _comboBoxItemsPlugged; }
            set
            {
                if (_comboBoxItemsPlugged != value)
                {

                    _comboBoxItemsPlugged = value;
                    OnPropertyChanged("comboBoxItemsPlugged");

                }
            }
        }

        private ComboBoxItem _selectedItemBattery;
        public ComboBoxItem selectedItemBattery
        {
            get { return _selectedItemBattery; }
            set
            {
                if (_selectedItemBattery != value)
                {

                    _selectedItemBattery = value;
                    OnPropertyChanged(nameof(selectedItemBattery));
                    setSave = "Save";
                }
            }
        }

        private ComboBoxItem _selectedItemPlugged;
        public ComboBoxItem selectedItemPlugged
        {
            get { return _selectedItemPlugged; }
            set
            {
                if (_selectedItemPlugged != value)
                {
                    _selectedItemPlugged = value;
                    OnPropertyChanged(nameof(selectedItemPlugged));
                    setSave = "Save";
                }
            }
        }

        private bool _cbRefreshRate;
        public bool cbRefreshRate
        {
            get { return _cbRefreshRate; }
            set
            {
                if (_cbRefreshRate != value)
                {
                    _cbRefreshRate = value;
                    OnPropertyChanged(nameof(cbRefreshRate));
                    setSave = "Save";
                }
            }
        }
  

        private bool _cbStartup;
        public bool cbStartup
        {
            get { return _cbStartup; }
            set
            {
                if (_cbStartup != value)
                {
                    _cbStartup = value;

                    OnPropertyChanged(nameof(cbStartup));
                    setSave = "Save";

                }
            }
        }


       // System.Windows.Threading.DispatcherTimer timer = new DispatcherTimer();
        private bool _labelfistLaunch = false;
        public SettingsViewVM()
        {
            // Avoiding Errors https://learn.microsoft.com/en-us/archive/technet-wiki/29874#avoiding-errors
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            _displaySpecs = new DisplaySpecs();
            _fileHandler = new Util.FileHandler();

           //  timer.Tick += new EventHandler(timerTick);
           //  timer.Interval = TimeSpan.FromSeconds(1);

            comboBoxItemsBattery = new ObservableCollection<ComboBoxItem>();
            comboBoxItemsPlugged = new ObservableCollection<ComboBoxItem>();

            if (_fileHandler.getBooleanSetting("firstLaunch"))
            {
                setConfigApp();
            }
            initializeItemsFromConfiguration();

            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }

        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            updateRefresh();

        }
        private void updateRefresh()
        {
            string currentRefreshRate =  getCurrentRefreshRate();

            if (currentRefreshRate + " Hz" != refreshRate)
            {
                bool canConvert = int.TryParse(currentRefreshRate, out int result);
                //  MessageBox.Show(refreshRate);
                refreshRate = canConvert ? result + " Hz" : currentRefreshRate + " Hz";
            }
        }


        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    updatePowerStatusAndIcon();
                    break;
                case PowerModes.Suspend:
                    break;
                case PowerModes.StatusChange:
                    updatePowerStatusAndIcon();
                    break;
                default:
                    break;
            }
        }
        private void updatePowerStatusAndIcon()
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;

            if (powerStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online)
            {
                changeRefeshRate(ChargingStatus.Charging);
                IconKind = PackIconMaterialDesignKind.BatteryChargingFull;
            }
            else
            {
                changeRefeshRate(ChargingStatus.Discharging);
                IconKind = PackIconMaterialDesignKind.BatteryFull;
            }
        }

        //private async void timerTick(object sender, EventArgs e)
        //{
        //    //int i = 0;
        //    timer.Stop();

        //    string currentRefreshRate = await Task.Run(() => getCurrentRefreshRate());

        //    if (currentRefreshRate + " Hz" != refreshRate)
        //    {
        //        //  MessageBox.Show(refreshRate);
        //        refreshRate = await Task.Run(() =>
        //        {
        //            bool canConvert = int.TryParse(currentRefreshRate, out int result);
        //            if (canConvert)
        //            {
        //                return result + " Hz";
        //            }
        //            else
        //            {
        //                return currentRefreshRate + " Hz";
        //            }
        //        });
        //    }

        //    timer.Start();

        //}


        private string getCurrentRefreshRate()
        {
            System.Management.ManagementClass wmi = new System.Management.ManagementClass("Win32_VideoController");
            var provider = wmi.GetInstances();
            foreach (var providers in provider)
            {
                int getCurrenRefresh = Convert.ToInt32(providers["CurrentRefreshRate"]);

                if (getCurrenRefresh != 0)
                {
                    return getCurrenRefresh.ToString();
                }
            }
            return "ERROR";
        }


        private void setConfigApp()
        {
            _labelfistLaunch = true;
            createShortcut();
            _fileHandler.setFileDisplay(_appPath);
            _fileHandler.getFile(_appPath);
            _fileHandler.setAppConfig("cbStartup", "False");
            _fileHandler.setAppConfig("cbRefreshRate", "True");

            string[] displayArray = ConfigurationManager.AppSettings["display"].Split(',');

            foreach (string item in displayArray)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    comboBoxItemsBattery.Add(new ComboBoxItem() { Content = item });
                    comboBoxItemsPlugged.Add(new ComboBoxItem() { Content = item });
                }
            }


            if (comboBoxItemsBattery.Count > 0 && comboBoxItemsPlugged.Count > 0)
            {
                _fileHandler.setAppConfig("comboBoxItemsBatterySelected", comboBoxItemsBattery[0].Content.ToString() ?? "");
                int lastIndex = comboBoxItemsPlugged.Count - 1;
                _fileHandler.setAppConfig("comboBoxItemsPluggedSelected", comboBoxItemsPlugged[lastIndex].Content.ToString() ?? "");


            }
            _fileHandler.setAppConfig("firstLaunch", "false");

        }

        private void initializeItemsFromConfiguration()
        {
            setSave = "Save";
            // _previousBatteryState =  getBatteryStatus(_appPath);
            if (!_labelfistLaunch)
            {
                string[] displayArray = ConfigurationManager.AppSettings["display"].Split(',');

                foreach (string item in displayArray)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        comboBoxItemsBattery.Add(new ComboBoxItem() { Content = item });
                        comboBoxItemsPlugged.Add(new ComboBoxItem() { Content = item });
                    }
                }
            }
            string configPluggedItem = ConfigurationManager.AppSettings["comboBoxItemsPluggedSelected"] ?? "";
            string configBatteryItem = ConfigurationManager.AppSettings["comboBoxItemsBatterySelected"] ?? "";
            if (!string.IsNullOrEmpty(configPluggedItem))
            {
                selectedItemPlugged = findItemComboBoxByString(comboBoxItemsPlugged, configPluggedItem);
            }
            if (!string.IsNullOrEmpty(configBatteryItem))
            {
                selectedItemBattery = findItemComboBoxByString(comboBoxItemsBattery, configBatteryItem);
            }

            cbRefreshRate = _fileHandler.getBooleanSetting("cbRefreshRate");
            cbStartup = _fileHandler.getBooleanSetting("cbStartup");
        //    timer.Start();
            updatePowerStatusAndIcon();
            updateRefresh();
        }
        private static ComboBoxItem findItemComboBoxByString(ObservableCollection<ComboBoxItem> items, string value)
        {
            foreach (ComboBoxItem item in items)
            {
                if (item.Content.ToString() == value)
                {
                    return item;
                }
            }
            return null;
        }

        private PackIconMaterialDesignKind _iconKind;
        public PackIconMaterialDesignKind IconKind
        {
            get { return _iconKind; }
            set
            {
                if (_iconKind != value)
                {
                    _iconKind = value;
                    OnPropertyChanged(nameof(IconKind));
                }
            }
        }


        public async void changeRefeshRate(ChargingStatus powerStatus)
        {
            if (_fileHandler.getBooleanSetting("cbRefreshRate") == false) return;

            // MessageBox.Show("test");
            string filePath = _appPath + @"/log.txt";
            string configPluggedItem = ConfigurationManager.AppSettings["comboBoxItemsPluggedSelected"] ?? "";
            string configBatteryItem = ConfigurationManager.AppSettings["comboBoxItemsBatterySelected"] ?? "";

            if (!string.IsNullOrEmpty(configBatteryItem) && !string.IsNullOrEmpty(configPluggedItem))
            {
                string lable;

                if (powerStatus == ChargingStatus.Discharging) lable = configBatteryItem;
                else lable = configPluggedItem;

                if (lable == getCurrentRefreshRate()) return;
                string qresDirectory = Path.Combine(_appPath, "ThirdPartyLibraries");
                var process = new Process();

                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    WorkingDirectory = qresDirectory,
                    Arguments = "@\"/C QRes.exe -R" + " " + lable,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                await _fileHandler.writeLog(filePath, "Change Refesh Rate to " + lable);

            }
        }

        public Util.RelayCommand closeCommand => new RelayCommand(execute => { close(); });
        public Util.RelayCommand exitCommand => new RelayCommand(execute => { exit(); });
        public Util.RelayCommand saveCommand => new RelayCommand(execute => { save(); });
        public Util.RelayCommand defautBtnCommand => new RelayCommand(execute => { setDefault(); });
        public Util.RelayCommand getDisplay => new RelayCommand(execute => { get(); });
        void get()
        {
            _fileHandler.setFileDisplay(_appPath);
            _fileHandler.getFile(_appPath);

            string[] displayArray = ConfigurationManager.AppSettings["display"].Split(',');
            comboBoxItemsBattery.Clear();
            comboBoxItemsPlugged.Clear();
            foreach (string item in displayArray)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    comboBoxItemsBattery.Add(new ComboBoxItem() { Content = item });
                    comboBoxItemsPlugged.Add(new ComboBoxItem() { Content = item });
                }
            }
            string configPluggedItem = ConfigurationManager.AppSettings["comboBoxItemsPluggedSelected"] ?? "";
            string configBatteryItem = ConfigurationManager.AppSettings["comboBoxItemsBatterySelected"] ?? "";
            if (!string.IsNullOrEmpty(configPluggedItem))
            {
                selectedItemPlugged = findItemComboBoxByString(comboBoxItemsPlugged, configPluggedItem);
            }
            if (!string.IsNullOrEmpty(configBatteryItem))
            {
                selectedItemBattery = findItemComboBoxByString(comboBoxItemsBattery, configBatteryItem);
            }

        }
        private void close()
        {
            var window = App.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            window?.Close();
            window?.Hide();
        }
        private void exit()
        {
            Environment.Exit(0);

        }

        private string _setSave;
        public string setSave
        {
            get { return _setSave; }
            set
            {
                if (_setSave != value)
                {
                    _setSave = value;
                    OnPropertyChanged(nameof(setSave));

                }
            }
        }

        public void setDefault()
        {
            cbRefreshRate = true;
            cbStartup = false;
        }
        private void SetAutoStart()
        {
            string appName = Application.ResourceAssembly.GetName().Name;
            const string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            var key = Registry.CurrentUser.OpenSubKey(path, true);
            if (key == null) return;
            if (cbStartup)
            {
                key.SetValue(appName, "\"" + _appPath + "\\" + Application.ResourceAssembly.GetName().Name + ".lnk" + "\"" + " /autoStartupAutoRefreshRate");
            }
            else
            {
                key.DeleteValue(appName, false);
            }
            key.Close();
        }

        private void createShortcut()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory; // Thư mục chứa ứng dụng hiện tại
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName; // Đường dẫn tới tệp exe của ứng dụng hiện tại
            string shortcutPath = Path.Combine(_appPath, Application.ResourceAssembly.GetName().Name + ".lnk"); // Đường dẫn tới tệp shortcut trên Desktop

            WshShell wshShell = new WshShell();

            IWshShortcut shortcut = (IWshShortcut)wshShell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = exePath;
            shortcut.WorkingDirectory = _appPath;
            shortcut.Description = "Auto Refesh Rate";
            shortcut.IconLocation = exePath;
            shortcut.Save();
        }
        private void save()
        {
            // MessageBox.Show(Path.Combine(_appPath, Application.ResourceAssembly.GetName().Name + ".lnk"));
            _previousBatteryState = 0;
            _fileHandler.setAppConfig("cbRefreshRate", cbRefreshRate.ToString());
            _fileHandler.setAppConfig("cbStartup", cbStartup.ToString());
            _fileHandler.setAppConfig("comboBoxItemsPluggedSelected", selectedItemPlugged.Content.ToString());
            _fileHandler.setAppConfig("comboBoxItemsBatterySelected", selectedItemBattery.Content.ToString());
            SetAutoStart();
            setSave = "Saved";
            updatePowerStatusAndIcon();
        }
    }
}

