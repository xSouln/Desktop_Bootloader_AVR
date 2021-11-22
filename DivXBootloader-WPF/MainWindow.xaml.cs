using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using xLib;
using xLibWpf.UI_Propertys;
using static DivXBootloader_WPF.Types;

namespace DivXBootloader_WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string FILENAME_COMPORT_OPTIONS = "ComPortOption.xDat";

        public ObservableCollection<UI_State> BootloderValues { get; set; } = new ObservableCollection<UI_State>();
        public byte[] FirmwareFile;

        public MainWindow()
        {
            xSupport.PointEntryUI = RequestThreadUI;
            xTracer.PointEntryUI = RequestThreadUI;

            Logger.Messagers.ReceivePacketInfo += ReceiverReceivePacketInfo;

            WindowTerminal.NoteInfo = xTracer.Notes;

            xComPort.Init((ComPortOption)xSerializer.OpenObject(FILENAME_COMPORT_OPTIONS));
            xComPort.Events.PacketReceived += Bootloader.DataReceiver;
            xComPort.StartUpdate(1000);
            
            xTcp.Events.PacketReceived += Bootloader.DataReceiver;
            xTcp.Init();

            InitializeComponent();
            
            MenuItemComPortOptions.Click += WindowComPortConnection.Open_Click;            
            MenuTcpOptions.Click += WindowTcpConnection.Open_Click;
            MenuItemTerminal.Click += WindowTerminal.Open_Click;
            MenuTcpTerminal.Click += WindowTerminal.Open_Click;

            MenuTcp.DataContext = xTcp.StateBackground;
            MenuComPort.DataContext = xComPort.ButBackground;
            xComPort.Events.Connected += comport_event_connected;
            xTcp.Events.Connected += tcp_event_connected;

            ButStartMain.DataContext = Bootloader.Loader.State;
            ButStartBoot.DataContext = Bootloader.Loader.State;
            ButFileOpen.DataContext = Bootloader.Loader.State;
            BotLoad.DataContext = Bootloader.Loader.State.Button;
            ButLink.DataContext = Bootloader.Communication.Background;
            
            BootloderValues.Add(Bootloader.Options.PageSize);
            BootloderValues.Add(Bootloader.Options.WordSize);
            BootloderValues.Add(Bootloader.Options.PagesCount);
            BootloderValues.Add(Bootloader.Info.LastWritePage);
            BootloderValues.Add(Bootloader.Loader.FirmwarePages);
            BootloderValues.Add(Bootloader.Info.ReadsPagesFull);
            BootloderValues.Add(Bootloader.FirmwareCrc);
            BootloderValues.Add(Bootloader.Info.ReadsFirmwareCrc);            
            BootloderValues.Add(Bootloader.Info.CalculatedCrc);           
            BootloderValues.Add(Bootloader.Handler.WriteComlite);
            BootloderValues.Add(Bootloader.Errors.Write);            
            BootloderValues.Add(Bootloader.Handler.CrcCalculated);
            BootloderValues.Add(Bootloader.Handler.CrcRead);
            BootloderValues.Add(Bootloader.Errors.Crc);
            BootloderValues.Add(Bootloader.Handler.EraseComlite);

            Bootloader.Init();
            
            Closed += MainClose;

            DataContext = this;
        }

        private void comport_event_connected(object arg, int key) { xTcp.Disconnect(); }
        private void tcp_event_connected(object arg, int key) { xComPort.Disconnect(); }

        private void MainClose(object sender, EventArgs e)
        {
            xComPort.Disconnect();
            xTcp.Disconnect();

            WindowComPortConnection.Close_Click();
            WindowTerminal.Dispose();
            WindowTcpConnection.Dispose();

            Bootloader.Dispose();

            xSerializer.SaveObject(xComPort.Option, FILENAME_COMPORT_OPTIONS);
        }

        private void RequestThreadUI(RequestUI request, object arg) { try { Dispatcher.Invoke(() => { request?.Invoke(arg); }); } catch { ReceiverTraceMessage("RequestThreadUI error"); } }

        private void ReceiverReceivePacketInfo(string note, string data, string convert_data)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (Logger.NoteReceivePacketInfo.Count > 2000) Logger.NoteReceivePacketInfo.RemoveAt(Logger.NoteReceivePacketInfo.Count - 1);
                    Logger.NoteReceivePacketInfo.Insert(0, new ReceivePacketInfo { Time = DateTime.Now.ToUniversalTime().ToString(), Note = note, Data = data, ConvertData = convert_data });
                });
            }
            catch { }
        }

        private void ReceiverTraceMessage(string note)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (Logger.NoteReceivePacketInfo.Count > 2000) Logger.NoteReceivePacketInfo.RemoveAt(Logger.NoteReceivePacketInfo.Count - 1);
                    Logger.NoteReceivePacketInfo.Insert(0, new ReceivePacketInfo { Time = DateTime.Now.ToUniversalTime().ToString(), Note = "Info:", Data = note });
                });
            }
            catch { }
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OPF = new OpenFileDialog();            
            string file_path;

            if (OPF.ShowDialog() == true)
            {
                file_path = OPF.FileName;
                if (Bootloader.OpenFile(file_path)) { TextBoxHex.Text = Bootloader.HexText; }
            }
            return;
        }

        private void ButStopLoad_Click(object sender, RoutedEventArgs e) { Bootloader.StopLoadFirmware(); }
        private void ButStartMain_Click(object sender, RoutedEventArgs e) { Bootloader.Try.StartMain(); }
        private void ButLoad_Click(object sender, RoutedEventArgs e)
        {
            if (Bootloader.Loader.State.Resolution) { Bootloader.LoadFirmware(); }
            else { Bootloader.StopLoadFirmware(); }
            //Bootloader.LoadFirmware(Bootloader.FirmwareFile);
        }

        private void ButStartBoot_Click(object sender, RoutedEventArgs e)
        {
            Bootloader.Try.StartBoot();
        }
    }
}
