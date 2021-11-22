using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xLib
{
    public static class xSupport
    {
        public static RequestAccessUI PointEntryUI;
        public static void RequestThreadUI(RequestUI request, object arg) { PointEntryUI?.Invoke(request, arg); }
        public static void RequestThreadUI(object arg) { if (arg != null) { RequestUI request = (RequestUI)arg; PointEntryUI?.Invoke(request, null); } }
    }
}
