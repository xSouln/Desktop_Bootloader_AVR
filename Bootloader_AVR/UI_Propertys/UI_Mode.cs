using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xLibWpf.UI_Propertys;
using static Bootloader_AVR.Types;

namespace Bootloader_AVR.UI_Propertys
{
    public class UI_Mode
    {
        private BOOT_MODE state;
        public UI_State IgnoreCrc = new UI_State { Name = nameof(IgnoreCrc) };
        public UI_State BootEnable = new UI_State { Name = nameof(BootEnable) };

        public BOOT_MODE Value
        {
            get { return state; }
            set
            {
                state = value;
                IgnoreCrc.State = Bootloader.IsEnable(BOOT_MODE.IGNORE_CRC);
                BootEnable.State = Bootloader.IsEnable(BOOT_MODE.BOOT_ENABLE);
            }
        }
    }
}
