using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace xLib
{
    /// <summary>
    /// Логика взаимодействия для WindowComPortConnection.xaml
    /// </summary>
    public partial class WindowComPortConnection : Window
    {
        public static WindowComPortConnection window_com_port_connection;
        public ObservableCollection<string> ComPortsList { get; set; }
        public List<int> BaudRateList { get; set; }
        public Brush ConnectionStateBackground { get; set; }

        public WindowComPortConnection()
        {
            InitializeComponent();

            //ConnectionStateBackground = xComPort.ButBackground.Background;
            LabelIndicatorConnectState.DataContext = xComPort.ButBackground;

            xComPort.Events.ChangeSelectedComPort += ChangeSelectedComPort;

            EndLineIdentifierTextBox.Text = xConverter.ToStrHex(xComPort.Option.LineEndIdentifier);
            BaudRateList = xComPort.BaudRateList;
            for (int i = 0; i < BaudRateList.Count; i++) if (xComPort.Option.BoadRate == BaudRateList[i]) BaudRateBox.SelectedIndex = i;
            for (int i = 0; i < xComPort.UpdateState.PortList.Count; i++) { if (xConverter.Compare(xComPort.UpdateState.PortList[i], xComPort.Option.LastComPortName)) FindComPortsBox.SelectedIndex = i; }
            EndLineTransmiterTextBox.Text = xConverter.ToStrHex(xComPort.Option.TransmitLineEnd);
            ComPortsList = xComPort.UpdateState.PortList;

            DataContext = this;
        }

        private void AcceptBut_Click(object sender, RoutedEventArgs e)
        {
            string EndLineIdentifier = xConverter.StrHexToStr(EndLineIdentifierTextBox.Text, "0x", " ");
            string EndLineTransmiter = xConverter.StrHexToStr(EndLineTransmiterTextBox.Text, "0x", " ");

            xComPort.Option.BoadRate = Convert.ToInt32(BaudRateBox.Text);
            if (xComPort.Port != null) xComPort.Port.BaudRate = xComPort.Option.BoadRate;
            if (EndLineIdentifier.Length > 0) xComPort.Option.LineEndIdentifier = EndLineIdentifier;
            if (EndLineTransmiter.Length > 0) xComPort.Option.TransmitLineEnd = EndLineTransmiter;
        }

        private void ChangeSelectedComPort(object Obj, int Key)
        {
            if (Key == 0) FindComPortsBox.SelectedIndex = -1;
            else if (Key == 1) FindComPortsBox.SelectedIndex = xComPort.UpdateState.PortList.Count - 1;
        }

        private void ConnectBut_Click(object sender, RoutedEventArgs e)
        {
            if (FindComPortsBox.SelectedIndex != -1) {
                xComPort.Connect(FindComPortsBox.Text);
            }
        }

        private void DisconnectBut_Click(object sender, RoutedEventArgs e)
        {
            xComPort.Disconnect();
        }

        private void BaudRateBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BaudRateBox.SelectedIndex != -1) xComPort.Option.BoadRate = (int)BaudRateBox.Items[BaudRateBox.SelectedIndex];
        }

        private void TransmitBut_Click(object sender, RoutedEventArgs e)
        {
            if (TransmitDataTextBox.Text.Length > 0) xComPort.Send(TransmitDataTextBox.Text);
        }

        private void FindComPortsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindComPortsBox.SelectedIndex != -1) xComPort.Option.LastComPortName = (string)FindComPortsBox.Items[FindComPortsBox.SelectedIndex];
        }

        public static void Open_Click(object sender, RoutedEventArgs e)
        {
            if (window_com_port_connection == null)
            {
                window_com_port_connection = new WindowComPortConnection();
                window_com_port_connection.Closed += new EventHandler(Close_Click);
                window_com_port_connection.Show();
            }
            else window_com_port_connection.Activate();
        }

        public static void Close_Click(object sender, EventArgs e)
        {
            window_com_port_connection?.Close();
            window_com_port_connection = null;
        }

        public static void Close_Click()
        {
            window_com_port_connection?.Close();
            window_com_port_connection = null;
        }
    }
}
