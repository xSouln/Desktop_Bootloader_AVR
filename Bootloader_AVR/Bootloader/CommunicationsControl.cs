using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using xLib;
using xLib.UI_Propertys;

namespace Bootloader_AVR
{
    public class CommunicationsControl
    {
        private const string BACKGROUND_TRUE = "#FF641818";
        private const string BACKGROUND_FALSE = "#FF21662A";

        public bool IsUpdate = false;
        public bool IsEnable = false;

        public Timer timer;
        public UI_Background Background { get; set; } = new UI_Background(BACKGROUND_TRUE);

        public void StartControl(int period) { timer = new Timer(update_state, null, period, period); }
        public void StopControl() { timer.Dispose(); }

        private void update_state(object arg)
        {
            if (!IsUpdate && !IsEnable) { IsEnable = true; xSupport.PointEntryUI(Background.Set, BACKGROUND_TRUE); }
            if (IsUpdate && IsEnable) { IsEnable = false; xSupport.PointEntryUI(Background.Set, BACKGROUND_FALSE); }
            IsUpdate = false;
        }
        public void Update() { IsUpdate = true; }
    }
}
