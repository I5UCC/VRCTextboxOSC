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
        private Timer timer;
        private UDPSender OscSender;
        private bool initialized = false;
        private IniData iniData;
        private FileIniDataParser parser = new FileIniDataParser();
        private readonly string CONFIGPATH = "config.ini";

        public MainWindow()
        {
            InitializeComponent();
            initialized = true;

            iniData = parser.ReadFile(CONFIGPATH);

            OscSender = new(iniData["Settings"]["IP"], int.Parse(iniData["Settings"]["Port"]));
            timer = new(double.Parse(iniData["Settings"]["Rate"]));
            this.TbxRate.Text = iniData["Settings"]["Rate"];
            CbxModes.SelectedIndex = int.Parse(iniData["Settings"]["Mode"]);
            
            Button_Send.Visibility = Visibility.Hidden;
            timer.Elapsed += Time_Elapsed;
        }

        private void Time_Elapsed(object? s, ElapsedEventArgs e) => SendMessage();

        private void Grid_MouseDown(object s, MouseButtonEventArgs e) => grid.Focus();

        private void Button_Clear_Click(object s, RoutedEventArgs e) => ClearMessage();

        private void Button_Send_Click(object s, RoutedEventArgs e) => SendMessage();

        private void TbxRate_LostFocus(object s, RoutedEventArgs e) => SettingsChanged();

        private void CbxModes_SelectionChanged(object s, SelectionChangedEventArgs e) => SettingsChanged();

        private void TextBox_TextChanged(object s, TextChangedEventArgs e)
        {
            if (initialized)
            {
                if (TbxMain.Text.Length == 0)
                {
                    ClearMessage();
                }
                else
                {
                    OscSender.Send(new OscMessage("/chatbox/typing", true));

                    if (this.CbxModes.SelectedIndex == 0)
                    {
                        timer.Stop();
                        timer.Start();
                    }
                }
            }
        }

        private void SettingsChanged()
        {
            if (initialized)
            {
                if (this.TbxRate.Text == "" || this.TbxRate.Text == "0")
                {
                    this.TbxRate.Text = "1";
                }

                iniData["Settings"]["Rate"] = this.TbxRate.Text;
                iniData["Settings"]["Mode"] = this.CbxModes.SelectedIndex.ToString();

                timer.Interval = Convert.ToDouble(this.TbxRate.Text);

                switch (this.CbxModes.SelectedIndex)
                {
                    case 0:
                        LblMs.Visibility = Visibility.Visible;
                        LblRate.Visibility = Visibility.Visible;
                        TbxRate.Visibility = Visibility.Visible;
                        this.Button_Send.Visibility = Visibility.Hidden;
                        break;
                    case 1:
                        LblMs.Visibility = Visibility.Hidden;
                        LblRate.Visibility = Visibility.Hidden;
                        TbxRate.Visibility = Visibility.Hidden;
                        this.Button_Send.Visibility = Visibility.Visible;
                        break;
                    default:
                        MessageBox.Show("HUH HOW DID YOU DO THAT?!?!?");
                        break;
                }
            }
        }

        private void TbxRate_TextChanged(object s, TextChangedEventArgs e)
        {
            if (initialized)
            {
                Regex reg = new("[^0-9]");
                if (reg.IsMatch(this.TbxRate.Text) && this.TbxRate.Text.Length != 0)
                {
                    this.TbxRate.Text = this.TbxRate.Text.Remove(this.TbxRate.Text.Length - 1);
                    this.TbxRate.Select(this.TbxRate.Text.Length, 0);
                }

                CheckRate();
            }
        }

        private void Textbox_KeyDown(object s, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && this.CbxModes.SelectedIndex == 1)
            {
                SendMessage();
            }
        }

        private void SendMessage() 
        {
            this.Dispatcher.Invoke(() =>
            {
                OscSender.Send(new OscMessage("/chatbox/typing", false));
                OscSender.Send(new OscMessage("/chatbox/input", TbxMain.Text, true));
                timer.Stop();
            });
        }

        private void ClearMessage()
        {
            TbxMain.Text = "";
            OscSender.Send(new OscMessage("/chatbox/input", "", true));
        }

        private void CheckRate()
        {
            if (this.TbxRate.Text != "" && Convert.ToInt32(this.TbxRate.Text) < 1000)
            {
                LblMs.Content = "ms (Could break)";
                LblMs.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                LblMs.Content = "ms";
                LblMs.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void Window_Closed(object s, EventArgs e)
        {
            parser.WriteFile(CONFIGPATH, iniData);
        }
    }
}
