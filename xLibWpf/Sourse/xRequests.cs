using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xLib;

namespace xLibWpf.Sourse
{
    public class xRequests
    {
        public bool IsEnable;
        public bool IsNotification;

        public byte[] Data;
        public int try_count = 0;

        public static unsafe byte[] prepare(string initial_word, RequestInfoT info, void* obj, string end_word)
        {
            byte[] buf = null;
            int size = 0;
            byte state = 0;
            if (initial_word != null && initial_word.Length > 0) { size += initial_word.Length; state |= 0b00000001; }
            size += sizeof(RequestInfoT);
            if (obj != null && info.Size > 0) { size += info.Size; state |= 0b00000010; }
            if (end_word != null && end_word.Length > 0) { size += end_word.Length; state |= 0b00000100; }

            buf = new byte[size];
            size = 0;
            if ((state & 0b00000001) > 0) { size = xCBase.MemCopy(buf, initial_word, 0); }
            size += xCBase.MemCopy(buf, &info, sizeof(RequestInfoT), size);
            if ((state & 0b00000010) > 0) { size += xCBase.MemCopy(buf, obj, info.Size, size); }
            if ((state & 0b00000100) > 0) { xCBase.MemCopy(buf, end_word, size); }

            return buf;
        }

        public unsafe bool Prepare(string initial_word, RequestInfoT info, void* obj)
        {
            if (!IsEnable)
            {
                Data = prepare(initial_word, info, obj, "\r");
                IsEnable = true;
                return true;
            }
            return false;
        }
        /*
        public unsafe bool Prepare(string data)
        {
            if (!IsEnable)
            {
                Data = Encoding.ASCII.GetBytes(data);
                IsEnable = true;
                return true;
            }
            return false;
        }
        */
        public unsafe bool Handler()
        {
            if (IsEnable)
            {
                xComPort.Send(Data);
                IsEnable = false;
                return true;
            }
            return false;
        }

        public void Accept() { IsEnable = false; }
    }
}
