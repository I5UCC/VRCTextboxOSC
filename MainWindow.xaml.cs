using System;
using System.Text.RegularExpressions;
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
        private Timer intervalTimer;
        private UDPSender oscSender;
        private bool isInitialized = false;
        private IniData iniData;
        private FileIniDataParser iniParser = new();
        private readonly string CONFIGPATH = "config.ini";

        public MainWindow()
        {
            InitializeComponent();
            isInitialized = true;

            iniData = iniParser.ReadFile(CONFIGPATH);

            oscSender = new(iniData["Settings"]["IP"], int.Parse(iniData["Settings"]["Port"]));

            intervalTimer = new(double.Parse(iniData["Settings"]["Rate"]));
            intervalTimer.Elapsed += Time_Elapsed;

            CbxModes.SelectedIndex = int.Parse(iniData["Settings"]["Mode"]);
            
            CkbxOverflow.IsChecked = bool.Parse(iniData["Settings"]["Continuous"]);

            SettingsChanged();
        }

        private void Window_Closed(object s, EventArgs e) => iniParser.WriteFile(CONFIGPATH, iniData);

        private void Grid_MouseDown(object s, MouseButtonEventArgs e) => grid.Focus();

        private void Time_Elapsed(object? s, ElapsedEventArgs e) => SendMessage(false);

        private void Button_Send_Click(object s, RoutedEventArgs e) => SendMessage(true);

        private void CheckBox_Checked(object sender, RoutedEventArgs e) => SettingsChanged();

        private void TbxRate_LostFocus(object s, RoutedEventArgs e) => SettingsChanged();

        private void CbxModes_SelectionChanged(object s, SelectionChangedEventArgs e) => SettingsChanged();

        private void Button_Clear_Click(object s, RoutedEventArgs e) => ClearMessage();

        private void Textbox_KeyDown(object s, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CbxModes.SelectedIndex == 1)
                SendMessage(true);
        }

        private void SendMessage(bool clear)
        {
            Dispatcher.Invoke(() =>
            {
                oscSender.Send(new OscMessage("/chatbox/typing", false));
                oscSender.Send(new OscMessage("/chatbox/input", TbxMain.Text, true));
                intervalTimer.Stop();
                if (clear)
                    TbxMain.Text = String.Empty;
            });


        }

        private void ClearMessage()
        {
            TbxMain.Text = String.Empty;
            oscSender.Send(new OscMessage("/chatbox/input", String.Empty, true));
            oscSender.Send(new OscMessage("/chatbox/typing", false));
            intervalTimer.Stop();
        }

        private void TbxMain_TextChanged(object s, TextChangedEventArgs e)
        {
            if (!isInitialized)
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
            if (!isInitialized)
                return;

            if (CkbxOverflow.IsChecked != null && (bool)CkbxOverflow.IsChecked)
            {
                iniData["Settings"]["Continuous"] = "true";
                TbxMain.MaxLength = int.Parse(iniData["VRCLimits"]["MaxLength"]) + 1;
                LblCharCount.Visibility = Visibility.Hidden;
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
                    LblCharCount.Visibility = Visibility.Hidden;
                    CkbxOverflow.Visibility = Visibility.Hidden;
                    Button_Send.Visibility = Visibility.Visible;
                    break;
                default:
                    MessageBox.Show("HUH HOW DID YOU DO THAT?!?!?");
                    break;
            }
        }
    }
}
