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
        private double rateLimit;
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

            TbxRate.Text = iniData["Settings"]["Rate"];
            CbxModes.SelectedIndex = int.Parse(iniData["Settings"]["Mode"]);
            
            rateLimit = int.Parse(iniData["VRCLimits"]["RateLimit"]);
            TbxMain.MaxLength = int.Parse(iniData["VRCLimits"]["MaxLength"]);

            Button_Send.Visibility = Visibility.Hidden;
        }

        private void Time_Elapsed(object? s, ElapsedEventArgs e) => SendMessage();

        private void Grid_MouseDown(object s, MouseButtonEventArgs e) => grid.Focus();

        private void Button_Clear_Click(object s, RoutedEventArgs e) => ClearMessage();

        private void Button_Send_Click(object s, RoutedEventArgs e) => SendMessage();

        private void TbxRate_LostFocus(object s, RoutedEventArgs e) => SettingsChanged();

        private void CbxModes_SelectionChanged(object s, SelectionChangedEventArgs e) => SettingsChanged();

        private void TextBox_TextChanged(object s, TextChangedEventArgs e)
        {
            if (isInitialized)
            {
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
        }

        private void SettingsChanged()
        {
            if (isInitialized)
            {
                if (TbxRate.Text == "" || TbxRate.Text == "0")
                    TbxRate.Text = "1";

                iniData["Settings"]["Rate"] = TbxRate.Text;
                iniData["Settings"]["Mode"] = CbxModes.SelectedIndex.ToString();

                intervalTimer.Interval = Convert.ToDouble(TbxRate.Text);

                switch (CbxModes.SelectedIndex)
                {
                    case 0:
                        LblMs.Visibility = Visibility.Visible;
                        LblRate.Visibility = Visibility.Visible;
                        TbxRate.Visibility = Visibility.Visible;
                        Button_Send.Visibility = Visibility.Hidden;
                        break;
                    case 1:
                        LblMs.Visibility = Visibility.Hidden;
                        LblRate.Visibility = Visibility.Hidden;
                        TbxRate.Visibility = Visibility.Hidden;
                        Button_Send.Visibility = Visibility.Visible;
                        break;
                    default:
                        MessageBox.Show("HUH HOW DID YOU DO THAT?!?!?");
                        break;
                }
            }
        }

        private void TbxRate_TextChanged(object s, TextChangedEventArgs e)
        {
            if (isInitialized)
            {
                
                Regex reg = new("[^0-9]");
                if (reg.IsMatch(TbxRate.Text) && TbxRate.Text.Length != 0)
                {
                    TbxRate.Text = TbxRate.Text.Remove(TbxRate.Text.Length - 1);
                    TbxRate.Select(TbxRate.Text.Length, 0);
                }

                if (TbxRate.Text != "" && Convert.ToInt32(TbxRate.Text) < rateLimit)
                {
                    LblMs.Content = String.Format("ms < {0} rate limit.", rateLimit);
                    LblMs.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    LblMs.Content = "ms";
                    LblMs.Foreground = new SolidColorBrush(Colors.White);
                }
            }
        }

        private void Textbox_KeyDown(object s, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CbxModes.SelectedIndex == 1)
                SendMessage();
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
            TbxMain.Text = "";
            oscSender.Send(new OscMessage("/chatbox/input", "", true));
            oscSender.Send(new OscMessage("/chatbox/typing", false));
            intervalTimer.Stop();
        }

        private void Window_Closed(object s, EventArgs e) => iniParser.WriteFile(CONFIGPATH, iniData);
    }
}
