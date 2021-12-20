using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xLibWpf.UI_Propertys;
using static Bootloader_AVR.Types;

namespace Bootloader_AVR.UI_Propertys
{
    public class UI_Errors
    {
        private BOOT_ERRORS state;
        public UI_State Crc = new UI_State { Name = nameof(Crc) +" error" };
        public UI_State Write = new UI_State { Name = nameof(Write) + " error" };
        public UI_State Read = new UI_State { Name = nameof(Read) + " error" };
        public UI_State Page = new UI_State { Name = nameof(Page) + " error" };

        public BOOT_ERRORS Value
        {
            get { return state; }
            set
            {
                state = value;
                Crc.State = Bootloader.IsEnable(BOOT_ERRORS.CRC);
                Write.State = Bootloader.IsEnable(BOOT_ERRORS.WRITE);
                Read.State = Bootloader.IsEnable(BOOT_ERRORS.READ);
                Page.State = Bootloader.IsEnable(BOOT_ERRORS.PAGE);
            }
        }
    }
}
