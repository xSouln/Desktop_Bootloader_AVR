using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xLib
{
    public class xRequest
    {
        private int size;
        private int offset;
        private byte[] data;

        public bool IsEnable;
        public bool IsNotification;

        public unsafe xRequest() { }

        public unsafe xRequest(string prefix, object key, int content_size, string end)
        {
            if (prefix == null) { prefix = ""; }
            if (end == null) { end = ""; }

            RequestInfoT info = new RequestInfoT { Key = (ushort)key, Size = (ushort)content_size };
            data = new byte[prefix.Length + sizeof(RequestInfoT) + content_size + end.Length];
            offset = prefix.Length + sizeof(RequestInfoT);
            size = content_size;

            xCBase.MemCopy(data, prefix, 0);
            xCBase.MemCopy(data, &info, sizeof(RequestInfoT), prefix.Length);
            xCBase.MemCopy(data, end, offset + content_size);
        }

        public unsafe bool Prepare(void* obj)
        {
            if (!IsEnable)
            {
                xCBase.MemCopy(data, obj, size, offset);
                IsEnable = true;
                return true;
            }
            return false;
        }

        public unsafe bool Prepare(byte[] obj) { fixed (byte* ptr = obj) { return Prepare(ptr); } }

        public unsafe bool Prepare()
        {
            if (!IsEnable) { IsEnable = true; return true; }
            return false;
        }

        public unsafe bool Handler()
        {
            if (IsEnable) {
                if (xComPort.Port != null && xComPort.Port.IsOpen) { xComPort.Send(data); }
                else if (xTcp.IsConnected) { xTcp.Send(data); }
                IsEnable = false;
                return true;
            }
            return false;
        }
    }
}
