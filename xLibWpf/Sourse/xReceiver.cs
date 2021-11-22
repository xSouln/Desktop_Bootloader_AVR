using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xLib
{
    public enum xRxResponse { Accept = 0, Storage = 1, BanTransition = 2 }

    public unsafe class xReceiverData
    {
        public xReceiver xRx;
        public byte* Content;
        public int Size;
    }

    public delegate void xReceiverCallback(xReceiverData ReceiverData);

    public class xReceiver
    {
        public struct xReceiverBufT
        {
            public byte[] Data;
            public ushort ByteRecived;
        }

        private xReceiverCallback packet_receive_callback;

        public byte[] end_line;
        public xReceiverBufT Buf = new xReceiverBufT();

        public xRxResponse Response = xRxResponse.Accept;
        public bool Loock = false;

        public xReceiver(ushort BufSize, byte[] EndLine, xReceiverCallback PacketReceiveCallback)
        {
            end_line = EndLine;
            packet_receive_callback = PacketReceiveCallback;
            Buf.Data = new byte[BufSize];
            Buf.ByteRecived = 0;

            if (end_line == null) end_line = new byte[] { (byte)'\r' };
        }
        private unsafe void BufLoaded()
        {
            if (packet_receive_callback != null)
            {
                Response = xRxResponse.Accept;
                xReceiverData data = new xReceiverData();
                data.xRx = this;
                data.Size = Buf.ByteRecived;
                fixed (byte* ptr = Buf.Data) { data.Content = ptr; packet_receive_callback(data); }
            }
            if (Response == xRxResponse.Accept) { Buf.ByteRecived = 0; }
        }

        private unsafe void EndLineIdentify()
        {
            if (packet_receive_callback != null)
            {
                Response = xRxResponse.Accept;
                xReceiverData data = new xReceiverData();
                data.xRx = this;
                data.Size = Buf.ByteRecived - end_line.Length;
                fixed (byte* ptr = Buf.Data) { data.Content = ptr; packet_receive_callback(data); }
            }
            if (Response == xRxResponse.Accept) { Buf.ByteRecived = 0; }
            else { }
        }

        public void Add(byte Data)
        {
            Buf.Data[Buf.ByteRecived] = Data;
            Buf.ByteRecived++;

            if (Buf.ByteRecived > end_line.Length) {
                int i = end_line.Length;
                int j = Buf.ByteRecived;
                while (i > 0)
                {
                    i--;
                    j--;
                    if (end_line[i] != Buf.Data[j]) { goto verify_end; }
                }
                EndLineIdentify();
            }

            verify_end:
            if (Buf.ByteRecived == Buf.Data.Length) { BufLoaded(); Buf.ByteRecived = 0; }
        }

        public void Clear()
        {
            Response = xRxResponse.Accept;
            Buf.ByteRecived = 0;
            Loock = false;
        }
    }
}
