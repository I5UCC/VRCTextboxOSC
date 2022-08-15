using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SharpOSC;
using IniParser;
using IniParser.Model;

namespace VRCTextboxOSC
{
    public partial class MainWindow : Window
    {
        private UDPSender oscSender;
        private Timer intervalTimer;
        
        private FileIniDataParser iniParser;
        private IniData iniData;

        private bool isEnabled;

        private readonly string CONFIGPATH = "config.ini";

        public MainWindow()
        {
            InitializeComponent();
            isEnabled = true;

            iniParser = new();
            iniData = iniParser.ReadFile(CONFIGPATH);

            LblStatus.Content = String.Format("Sending to {0}:{1}", iniData["Settings"]["IP"], iniData["Settings"]["Port"]);
            oscSender = new(iniData["Settings"]["IP"], int.Parse(iniData["Settings"]["Port"]));

            intervalTimer = new(double.Parse(iniData["Settings"]["Rate"]));
            intervalTimer.Elapsed += Time_Elapsed;

            CbxModes.SelectedIndex = int.Parse(iniData["Settings"]["Mode"]);
            
            CkbxOverflow.IsChecked = bool.Parse(iniData["Settings"]["Continuous"]);

            SettingsChanged();
        }

        private void Window_Closed(object s, EventArgs e) => iniParser.WriteFile(CONFIGPATH, iniData);

        private void CheckBox_Checked(object sender, RoutedEventArgs e) => SettingsChanged();

        private void CbxModes_SelectionChanged(object s, SelectionChangedEventArgs e) => SettingsChanged();

        private void Button_Clear_Click(object s, RoutedEventArgs e) => ClearMessage();

        private void Time_Elapsed(object? s, ElapsedEventArgs e) => SendMessage();

        private void Button_Send_Click(object s, RoutedEventArgs e)
        {
            SendMessage();
            isEnabled = false;
            TbxMain.Clear();
            isEnabled = true;
        }

        private void Textbox_KeyDown(object s, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CbxModes.SelectedIndex == 1)
            {
                SendMessage();
                isEnabled = false;
                TbxMain.Clear();
                isEnabled = true;
            }
            else if (e.Key == Key.Enter && CbxModes.SelectedIndex == 0)
            {
                SendMessage();
                ClearMessage();
            }
        }

        private void SendMessage()
        {
            Dispatcher.Invoke(() =>
            {
                oscSender.Send(new OscMessage("/chatbox/typing", false));
                oscSender.Send(new OscMessage("/chatbox/input", TbxMain.Text, true));
                intervalTimer.Stop();
            });
        }

        private void ClearMessage()
        {
            TbxMain.Clear();
            oscSender.Send(new OscMessage("/chatbox/input", String.Empty, true));
            oscSender.Send(new OscMessage("/chatbox/typing", false));
            intervalTimer.Stop();
        }

        private void TbxMain_TextChanged(object s, TextChangedEventArgs e)
        {
            if (!isEnabled)
                return;

            if (TbxMain.MaxLength == TbxMain.Text.Length && CkbxOverflow.IsChecked != null && (bool)CkbxOverflow.IsChecked)
            {
                int delIndex = TbxMain.Text.IndexOf(' ') + 1;

                if (delIndex == 0)
                    TbxMain.Text = TbxMain.Text.Remove(TbxMain.Text.Length - 1);
                else
                    TbxMain.Text = TbxMain.Text[delIndex..];

                TbxMain.Select(TbxMain.Text.Length, 0);
            }

            if (TbxMain.Text.Length == 0)
            {
                ClearMessage();
            }
            else
            {
                oscSender.Send(new OscMessage("/chatbox/typing", true));

                if (CbxModes.SelectedIndex == 0)
                    intervalTimer.Start();
            }

            LblCharCount.Content = String.Format("{0}/{1}", TbxMain.Text.Length, TbxMain.MaxLength);
            if (TbxMain.Text.Length >= 120)
                LblCharCount.Foreground = new SolidColorBrush(Colors.Red);
            else if (TbxMain.Text.Length >= 90)
                LblCharCount.Foreground = new SolidColorBrush(Colors.Orange);
            else
                LblCharCount.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
        }

        private void SettingsChanged()
        {
            if (!isEnabled)
                return;

            if (CkbxOverflow.IsChecked != null && (bool)CkbxOverflow.IsChecked)
            {
                iniData["Settings"]["Continuous"] = "true";
                LblCharCount.Visibility = Visibility.Hidden;
                TbxMain.MaxLength = int.Parse(iniData["VRCLimits"]["MaxLength"]) + 1;
            }
            else
            {
                iniData["Settings"]["Continuous"] = "false";
                LblCharCount.Visibility = Visibility.Visible;
                TbxMain.MaxLength = int.Parse(iniData["VRCLimits"]["MaxLength"]);
            }

            iniData["Settings"]["Mode"] = CbxModes.SelectedIndex.ToString();

            switch (CbxModes.SelectedIndex)
            {
                case 0:
                    CkbxOverflow.Visibility = Visibility.Visible;
                    Button_Send.Visibility = Visibility.Hidden;
                    break;
                case 1:
                    LblCharCount.Visibility = Visibility.Visible;
                    CkbxOverflow.Visibility = Visibility.Hidden;
                    Button_Send.Visibility = Visibility.Visible;
                    break;
                default:
                    MessageBox.Show("HUH HOW DID YOU DO THAT?!?!?");
                    break;
            }

            TbxMain.Focus();
        }
    }
}
