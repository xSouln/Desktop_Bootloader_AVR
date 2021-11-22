using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using xLib.UI_Propertys;

namespace xLib
{
    [Serializable]
    public class ComPortOption
    {
        public int BoadRate = 115200;
        public string LastComPortName = "";
        public string LineEndIdentifier = "\r\n";
        public string LastConnectedComPort = "";
        public string TransmitLineEnd = "\r\n";
    }

    public class ComPortUpdateState
    {             
        public bool LastState = false;
        public ObservableCollection<string> PortList = new ObservableCollection<string>();

        public bool IsFind;
        public bool IsRemove;
    }

    public unsafe class ComPortTransmitedData
    {
        public string InitialWord;
        public void* xObject;
        public int xObjectSize;
        public string EndWord;
    }

    public delegate void ComPortCommandRecivedCallback(List<byte[]> CommandList);

    public unsafe delegate bool DataTransmitter(string InitialWord, byte[] data, string EndWord);
    public static class xComPort
    {
        public static class Events
        {
            public static xEventCallback Connected;
            public static xEventCallback Disconnected;
            public static xReceiverCallback PacketReceived;
            public static xEventCallback ChangeSelectedComPort;
        }

        private const string BACKGROUND_CONNECTED = "#FF21662A";
        private const string BACKGROUND_DISCONNECTED = "#FF641818";
        public static UI_Background ButBackground { get; set; } = new UI_Background(BACKGROUND_DISCONNECTED);

        public const int PORT_ADD = 1;
        public const int PORT_REMOVE = 0;

        public static EventTracer Tracer;

        private static Thread RxThread;
        private static TimerCallback UpdateStateTimerCallback;
        private static Timer UpdateStateTimer;

        public static ComPortOption Option = new ComPortOption();
        public static SerialPort Port;
        public static ComPortUpdateState UpdateState;
        public static bool IsConnected;

        private static xReceiver xRx = new xReceiver(5000, new byte[] { (byte)'\r' }, packet_received);

        public static List<int> BaudRateList = new List<int>() { 9600, 38400, 115200, 128000, 256000, 521600, 900000, 921600 };

        public static void Init(ComPortOption option)
        {
            if (option != null) Option = option;
            else Option = new ComPortOption();
            UpdateStateTimerCallback = new TimerCallback(ComPortUpdate);
            UpdateState = new ComPortUpdateState();

            Events.Disconnected += ComPortDisconnected;
            Events.Connected += ComPortConnected;

            ButBackground = new UI_Background(BACKGROUND_DISCONNECTED);
        }

        private static void ComPortDisconnected(object arg, int key) { IsConnected = false; xSupport.RequestThreadUI(ButBackground.Set, BACKGROUND_DISCONNECTED); }
        private static void ComPortConnected(object arg, int key) { IsConnected = true; xSupport.RequestThreadUI(ButBackground.Set, BACKGROUND_CONNECTED); }
        private static void trace(string note) { Tracer?.Invoke(note); xTracer.Message(note); }
        private static void packet_received(xReceiverData ReceiverData) { Events.PacketReceived?.Invoke(ReceiverData); }

        private static void ReciveDataThread()
        {
            xRx.Clear();
            try
            {
                while (true) { if (Port.BytesToRead > 0 && !xRx.Loock) { xRx.Add((byte)Port.ReadByte()); } }
            }
            catch(Exception e)
            {
                trace(e.ToString());
                Events.Disconnected?.Invoke(0, 0);
                Port?.Close();                
            }
        }

        public static bool Connect(string PortName, int BoadRate)
        {
            if (PortName == null || PortName.Length < 3) return false;

            try
            {
                Port = new SerialPort(PortName, BoadRate, Parity.None, 8, StopBits.One);
                Port.Encoding = Encoding.GetEncoding("iso-8859-1");
            
                Port.Open();
                RxThread = new Thread(new ThreadStart(ReciveDataThread));
                RxThread.Start();

                Option.BoadRate = BoadRate;
                Option.LastConnectedComPort = PortName;

                Events.Connected?.Invoke(0, 0);
                return true;
            }
            catch
            {
                Events.Disconnected?.Invoke(0, 0);
                trace("error connect");
                return false;
            }

        }
        
        public static bool Connect(string PortName)
        {
            if (Port == null || !Port.IsOpen) return Connect(PortName, Option.BoadRate);
            else return false;
        }
        
        public static void Disconnect()
        {
            try
            {
                if (Port != null && Port.IsOpen)
                {
                    Events.Disconnected?.Invoke(0, 0);
                    RxThread.Abort();
                    Port.Close();
                    Port = null;
                }
            }
            catch { Events.Disconnected?.Invoke(0, 0); }
        }

        private static void ComPortUpdate(object obj)
        {
            if (Port != null) {
                if (UpdateState.LastState != Port.IsOpen)
                {
                    UpdateState.LastState = Port.IsOpen;
                    if (Port.IsOpen) Events.Connected?.Invoke(0, 0);
                    else Events.Disconnected?.Invoke(0, 0);
                }
            }

            string[] Ports = SerialPort.GetPortNames().ToList<string>().ToArray();
           
            UpdateState.IsFind = false;
            UpdateState.IsRemove = false;

            try
            {
                for (int com_number = 0; com_number < Ports.Length; com_number++)
                {
                    if (!xConverter.Compare(Ports[com_number], UpdateState.PortList.ToArray()))
                    {
                        trace(Ports[com_number] + " find");
                        UpdateState.PortList.Add(Ports[com_number]);
                        if (Option?.LastComPortName.Length > 3)
                        {
                            if (xConverter.Compare(Option?.LastComPortName, Ports[com_number])) Events.ChangeSelectedComPort?.Invoke(Option?.LastComPortName, PORT_ADD);
                        }
                        UpdateState.IsFind = true;

                    }
                }

                for (int com_number = 0; com_number < UpdateState.PortList.Count; com_number++)
                {
                    if (!xConverter.Compare(UpdateState.PortList[com_number], Ports))
                    {
                        trace(UpdateState.PortList[com_number] + " disconnect to pk");
                        if (Option?.LastComPortName.Length > 3)
                        {
                            if (xConverter.Compare(Option?.LastComPortName, Ports[com_number])) Events.ChangeSelectedComPort?.Invoke(Option?.LastComPortName, PORT_REMOVE);
                        }
                        UpdateState.PortList.RemoveAt(com_number);
                        UpdateState.IsRemove = true;
                    }
                }
            }
            catch { }
        }

        public static void StartUpdate(int period) { UpdateStateTimer = new Timer(UpdateStateTimerCallback, null, 0, period); }

        public static bool Send(string str)
        {
            if (Port == null || !Port.IsOpen || str == null || str.Length == 0) return false;

            Port.Write(str);
            return true;
        }

        public static bool Send(byte[] data)
        {
            if (Port != null && Port.IsOpen && data != null && data.Length > 0) { Port.Write(data, 0, data.Length); return true; }
            return false;
        }

        public static bool Send(string InitialWord, byte[] data, string EndWord)
        {
            int PacketSize = 0;
            byte[] buf;
            byte state = 0;
            if (InitialWord != null && InitialWord.Length > 0) { PacketSize += InitialWord.Length; state |= 0b00000001; }
            if (data != null && data.Length > 0) { PacketSize += data.Length; state |= 0b00000010; }
            if (EndWord != null && EndWord.Length > 0) { PacketSize += EndWord.Length; state |= 0b00000100; }
            if(PacketSize > 0)
            {
                buf = new byte[PacketSize];
                if ((state & 0b00000001) > 0) PacketSize = xCBase.MemCopy(buf, InitialWord, 0);
                if ((state & 0b00000010) > 0) PacketSize += xCBase.MemCopy(buf, data, PacketSize);
                if ((state & 0b00000100) > 0) PacketSize += xCBase.MemCopy(buf, EndWord, PacketSize);
                if (Port != null && Port.IsOpen) { Port?.Write(buf, 0, PacketSize); return true; }
            }
            return false;
        }

        public static unsafe bool Send(string InitialWord, void* xObject, int xObjectSize, string EndWord)
        {
            int PacketSize = 0;
            byte[] buf;
            byte state = 0;
            if (InitialWord != null && InitialWord.Length > 0) { PacketSize += InitialWord.Length; state |= 0b00000001; }
            if (xObject != null && xObjectSize > 0) { PacketSize += xObjectSize; state |= 0b00000010; }
            if (EndWord != null && EndWord.Length > 0) { PacketSize += EndWord.Length; state |= 0b00000100; }
            if (PacketSize > 0)
            {
                buf = new byte[PacketSize];
                if ((state & 0b00000001) > 0) PacketSize = xCBase.MemCopy(buf, InitialWord, 0);
                if ((state & 0b00000010) > 0) PacketSize += xCBase.MemCopy(buf, xObject, xObjectSize, PacketSize);
                if ((state & 0b00000100) > 0) PacketSize += xCBase.MemCopy(buf, EndWord, PacketSize);
                if (Port != null && Port.IsOpen) { Port?.Write(buf, 0, PacketSize); return true; }
            }
            return false;
        }

        public static unsafe bool Send(string InitialWord, RequestInfoT Info, void* xObject, int xObjectSize, string EndWord)
        {
            int PacketSize = 0;
            byte[] buf;
            byte state = 0;
            if (InitialWord != null && InitialWord.Length > 0) { PacketSize += InitialWord.Length; state |= 0b00000001; }
            if (xObject != null && xObjectSize > 0) { PacketSize += xObjectSize; state |= 0b00000010; }
            if (EndWord != null && EndWord.Length > 0) { PacketSize += EndWord.Length; state |= 0b00000100; }
            if (PacketSize > 0)
            {
                buf = new byte[PacketSize + sizeof(RequestInfoT)];
                PacketSize = 0;
                if ((state & 0b00000001) > 0) PacketSize = xCBase.MemCopy(buf, InitialWord, 0);
                PacketSize += xCBase.MemCopy(buf, &Info, sizeof(RequestInfoT), PacketSize);
                if ((state & 0b00000010) > 0) PacketSize += xCBase.MemCopy(buf, xObject, xObjectSize, PacketSize);
                if ((state & 0b00000100) > 0) PacketSize += xCBase.MemCopy(buf, EndWord, PacketSize);
                if (Port != null && Port.IsOpen) { Port?.Write(buf, 0, PacketSize); return true; }
            }
            return false;
        }

        public static unsafe bool Send(byte[] InitialWord, void* xObject, int xObjectSize, string EndWord)
        {
            int PacketSize = 0;
            byte[] buf;
            byte state = 0;
            if(Port != null && Port.IsOpen)
            {
                if (InitialWord != null && InitialWord.Length > 0) { PacketSize += InitialWord.Length; state |= 0b00000001; }
                if (xObject != null && xObjectSize > 0) { PacketSize += xObjectSize; state |= 0b00000010; }
                if (EndWord != null && EndWord.Length > 0) { PacketSize += EndWord.Length; state |= 0b00000100; }
                if (PacketSize > 0)
                {
                    buf = new byte[PacketSize];
                    if ((state & 0b00000001) > 0) PacketSize = xCBase.MemCopy(buf, InitialWord, 0);
                    if ((state & 0b00000010) > 0) PacketSize += xCBase.MemCopy(buf, xObject, xObjectSize, PacketSize);
                    if ((state & 0b00000100) > 0) PacketSize += xCBase.MemCopy(buf, EndWord, PacketSize);
                    try { Port?.Write(buf, 0, PacketSize); return true; }
                    catch { }
                }
            }            
            return false;
        }

        public static unsafe bool Send(xRequestT Request, void* xObject, int xObjectSize)
        {
            byte[] InitialWord;
            if (Request.Prefix == null) Request.Prefix = "";
            InitialWord = new byte[Request.Prefix.Length + sizeof(RequestInfoT)];
            xCBase.MemCopy(InitialWord, Request.Prefix, 0);
            xCBase.MemCopy(InitialWord, &Request.Info, sizeof(RequestInfoT), Request.Prefix.Length);
            return Send(InitialWord, xObject, xObjectSize, Request.End);
        }
    }
}
