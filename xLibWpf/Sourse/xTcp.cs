using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using xLib;
using xLib.UI_Propertys;

namespace xLib
{
    public static class xTcp
    {
        private const string BACKGROUND_CONNECTED = "#FF21662A";
        private const string BACKGROUND_DISCONNECTED = "#FF641818";
        public static class Events
        {
            public static xEventCallback Disconnected;
            public static xEventCallback Connected;
            public static xReceiverCallback PacketReceived;
        }

        public static EventTracer Tracer;
        private static TcpClient client;
        private static NetworkStream stream;
        private static Thread server_thread;

        public static string LastAddress;
        public static string Ip;
        public static int Port;
        public static bool IsConnected = false;

        public static UI_Background StateBackground { get; set; } = new UI_Background(BACKGROUND_DISCONNECTED);

        private static xReceiver xRx = new xReceiver(10000, new byte[] { (byte)'\r' }, packet_received);
        private static void trace(string note) { Tracer?.Invoke(note); xTracer.Message(note); }
        private static void event_connected(object arg, int key) { IsConnected = true; xSupport.RequestThreadUI(StateBackground.Set, BACKGROUND_CONNECTED); }
        private static void event_disconnected(object arg, int key) { IsConnected = false; xSupport.RequestThreadUI(StateBackground.Set, BACKGROUND_DISCONNECTED); }
        private static void packet_received(xReceiverData ReceiverData) { Events.PacketReceived?.Invoke(ReceiverData); }

        public static void Init()
        {
            Events.Connected += event_connected;
            Events.Disconnected += event_disconnected;
            xRx.Clear();
        }

        private static void rx_thread()
        {
            try
            {
                stream = client.GetStream();
                Events.Connected?.Invoke(null, 0);
                trace("tcp: thread start");

                int count = 0;
                byte[] buf = new byte[10000];
                xRx.Clear();

                while (true)
                {
                    do
                    {
                        count = stream.Read(buf, 0, buf.Length);
                        if (count > 0) { for (int i = 0; i < count; i++) xRx.Add(buf[i]); }
                    }
                    while ((bool)stream?.DataAvailable);
                }
            }
            catch (Exception e) { trace(e.ToString()); thread_close(); }
        }

        private static bool thread_close()
        {
            if (stream != null) { stream.Flush(); stream.Close(); stream = null; }
            if (client != null)
            {
                client.Client?.Close();
                client.Close();
                client = null;
            }
            server_thread?.Abort();
            server_thread = null;
            trace("tcp: thread close");
            Events.Disconnected?.Invoke(null, 0);
            return true;
        }

        private static void request_callback(IAsyncResult ar)
        {
            try
            {
                client = (TcpClient)ar.AsyncState;
                if (client != null) { trace("tcp: client connected"); server_thread = new Thread(new ThreadStart(rx_thread)); server_thread.Start(); }
                else trace("tcp: client connect error");
            }
            catch (Exception ex)
            {
                trace(ex.ToString());
                trace("tcp: client connect abort");
                thread_close();
                return;
            }
        }

        public static void Connect(string address)
        {
            string[] strs;

            if (client != null) { trace("tcp: device is connected"); return; }
            trace("tcp: request connect");

            if (address.Length < 9) { trace("tcp: incorrect parameters"); return; }
            strs = address.Split('.');
            if (strs.Length < 4) { trace("tcp: incorrect parameters"); return; }

            strs = address.Split(':');
            if (strs.Length != 2) { trace("tcp: incorrect parameters"); return; }

            int port = Convert.ToInt32(strs[1]);
            string ip = strs[0];

            Ip = ip;
            Port = port;
            client = new TcpClient();

            LastAddress = address;

            trace("tcp: client begin connect");
            IAsyncResult result = client.BeginConnect(Ip, Port, request_callback, client);
        }
        //=====================================================================================================================
        public static void Disconnect()
        {
            trace("tcp: request disconnect");
            thread_close();
        }
        //=====================================================================================================================
        public static bool Send(string str)
        {
            if (client != null && stream != null && client.Connected)
            {
                byte[] data = Encoding.UTF8.GetBytes(str + "\r");
                trace("tcp send: " + str);

                try { stream.WriteAsync(data, 0, data.Length); return true; }
                catch { trace("tcp: невозможно отправить на указаный ip"); return false; }
            }
            trace("tcp: нет соединения");
            return false;
        }
        //=====================================================================================================================
        public static bool Send(byte[] data)
        {
            if (client != null && stream != null && client.Connected && data != null && data.Length > 0)
            {
                try { stream.WriteAsync(data, 0, data.Length); return true; }
                catch { trace("tcp: невозможно отправить на указаный ip"); return false; }
            }
            trace("tcp: нет соединения");
            return false;
        }
        //=====================================================================================================================
    }
}
