using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace xLib
{
    public delegate void EventTracer(string data);

    public delegate void xEventCallback(object arg, int key);

    public delegate void xEvent(object arg);

    public unsafe delegate void xReceiverEvent(xCBaseCallback Packet, CBASE_KEY Key);

    public delegate void RequestUI(object arg);

    public delegate void RequestAccessUI(RequestUI request, object arg);

    public struct RequestInfoT { public ushort Key; public ushort Size; }
    public struct ResponseInfoT { public ushort Key; public ushort Size; }

    public struct ResponsePrefixT
    {
        public byte Start;
        public byte Ch1;
        public byte Ch2;
        public byte Ch3;
        public byte Ch4;
        public byte End;
    }

    public struct RequestPrefixT
    {
        public byte Start;
        public byte Ch1;
        public byte Ch2;
        public byte Ch3;
        public byte Ch4;
        public byte End;
    }

    public struct ResponseT
    {
        public ResponsePrefixT Prefix;
        public ResponseInfoT Info;
    }

    public struct RequestT
    {
        public RequestPrefixT Prefix;
        public RequestInfoT Info;
    }

    public struct xRequestT
    {
        public string Prefix;
        public RequestInfoT Info;
        public string End;
    }

    public struct xResponseT
    {
        public string Prefix;
        public ResponseInfoT Info;
        public string End;
    }
}
