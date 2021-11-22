using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xLib.Sourse
{
    public class xResponse
    {
        public xCommand Command;
        public unsafe xResponse(string prefix, object key, int content_size, CBASE_MODE mode)
        {            
            if (prefix == null) { prefix = ""; }
            Command = new xCommand();
            Command.ContentSize = content_size;
            Command.Mode = mode;
            Command.Key = new byte[prefix.Length + sizeof(ResponseInfoT)];
            Command.Offset = sizeof(ushort);
            ResponseInfoT Info = new ResponseInfoT { Key = (ushort)key, Size = (ushort)content_size };

            xCBase.MemCopy(Command.Key, prefix, 0);
            xCBase.MemCopy(Command.Key, &Info, sizeof(ResponseInfoT), prefix.Length);            
        }

        public unsafe xResponse(string prefix)
        {
            if (prefix == null) { prefix = ""; }
            Command = new xCommand();
            Command.ContentSize = 0;
            Command.Mode = CBASE_MODE.CONTENT;
            Command.Key = new byte[prefix.Length];

            xCBase.MemCopy(Command.Key, prefix, 0);
        }

        public unsafe xResponse(string prefix, object key, int content_size)
        {
            if (prefix == null) { prefix = ""; }
            Command = new xCommand();
            Command.ContentSize = content_size;
            Command.Mode = CBASE_MODE.OBJECT;
            Command.Key = new byte[prefix.Length + sizeof(ResponseInfoT)];
            ResponseInfoT Info = new ResponseInfoT { Key = (ushort)key, Size = (ushort)content_size };

            xCBase.MemCopy(Command.Key, prefix, 0);
            xCBase.MemCopy(Command.Key, &Info, sizeof(ResponseInfoT), prefix.Length);
        }
    }
}
