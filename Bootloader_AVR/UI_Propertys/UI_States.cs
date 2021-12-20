using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xLib;
using xLibWpf.UI_Propertys;
using static DivXBootloader_WPF.Types;

namespace DivXBootloader_WPF.UI_Propertys
{
    public class UI_States
    {
        public class UI_StatesErrors
        {
            public UI_State Crc = new UI_State { Name = nameof(Crc) + " error" };
            public UI_State Write = new UI_State { Name = nameof(Write) + " error" };
            public UI_State Read = new UI_State { Name = nameof(Read) + " error" };
        }

        public class UI_StatesHandler
        {
            public UI_State IsEnable = new UI_State { Name = nameof(IsEnable) };
            public UI_State IsReads = new UI_State { Name = nameof(IsReads) };
            public UI_State IsWrites = new UI_State { Name = nameof(IsWrites) };
            public UI_State WriteComlite = new UI_State { Name = "Write comlite" };
            public UI_State ReadComlite = new UI_State { Name = nameof(ReadComlite) };
            public UI_State EraseComlite = new UI_State { Name = nameof(EraseComlite) };
            public UI_State CalculateCrcComlite = new UI_State { Name = "Calculate crc comlite" };
            public UI_State ReadCrcComlite = new UI_State { Name = "Read crc comlite" };
        }

        public class UI_StatesMode
        {
            public UI_State IgnoreCrc = new UI_State { Name = nameof(IgnoreCrc) };
            public UI_State BootEnable = new UI_State { Name = nameof(BootEnable) };
        }

        private BootloaderStatesT states = new BootloaderStatesT();
        public UI_StatesErrors Errors = new UI_StatesErrors();
        public UI_StatesHandler Handler = new UI_StatesHandler();
        public UI_StatesMode Mode = new UI_StatesMode();

        private bool IsEnable(BOOT_ERRORS_E state) { return (states.Errors & state) == state; }
        private bool IsEnable(BOOT_HANDLER_E state) { return (states.Handler & state) == state; }
        private bool IsEnable(BOOT_MODE_E state) { return (states.Mode & state) == state; }

        public BootloaderStatesT Value
        {
            set
            {
                states = value;
                Errors.Crc.State = IsEnable(BOOT_ERRORS_E.CRC);
                Errors.Read.State = IsEnable(BOOT_ERRORS_E.READ);
                Errors.Write.State = IsEnable(BOOT_ERRORS_E.WRITE);

                Handler.IsEnable.State = IsEnable(BOOT_HANDLER_E.IS_ENABLE);
                Handler.IsReads.State = IsEnable(BOOT_HANDLER_E.IS_READS);
                Handler.IsWrites.State = IsEnable(BOOT_HANDLER_E.IS_WRITES);
                Handler.WriteComlite.State = IsEnable(BOOT_HANDLER_E.WRITE_COMPLITE);
                Handler.ReadComlite.State = IsEnable(BOOT_HANDLER_E.READ_COMLITE);
                Handler.EraseComlite.State = IsEnable(BOOT_HANDLER_E.ERASE_COMLITE);
                Handler.CalculateCrcComlite.State = IsEnable(BOOT_HANDLER_E.CALCULATE_CRC_COMLITE);
                Handler.ReadCrcComlite.State = IsEnable(BOOT_HANDLER_E.READ_CRC_COMLITE);

                Mode.IgnoreCrc.State = IsEnable(BOOT_MODE_E.IGNORE_CRC);
                Mode.BootEnable.State = IsEnable(BOOT_MODE_E.BOOT_ENABLE);
            }
            get { return states; }
        }
    }
}
