using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using xLibWpf.UI_Propertys;
using static DivXBootloader_WPF.Types;

namespace DivXBootloader_WPF.UI_Propertys
{
    public class UI_Handler
    {
        private BOOT_HANDLER state;

        public UI_State IsEnable = new UI_State { Name = nameof(IsEnable) };
        public UI_State IsReads = new UI_State { Name = nameof(IsReads) };
        public UI_State IsWrites = new UI_State { Name = nameof(IsWrites) };
        public UI_State WriteComlite = new UI_State { Name = nameof(WriteComlite) };
        public UI_State ReadComlite = new UI_State { Name = nameof(ReadComlite) };
        public UI_State EraseComlite = new UI_State { Name = nameof(EraseComlite) };
        public UI_State CrcRead = new UI_State { Name = nameof(CrcRead) };
        public UI_State CrcCalculated = new UI_State { Name = nameof(CrcCalculated) };
        public UI_State MainStartated = new UI_State { Name = nameof(MainStartated) };

        public BOOT_HANDLER Value
        {
            get { return state; }
            set
            {
                state = value;
                IsEnable.State = Bootloader.IsEnable(BOOT_HANDLER.IS_ENABLE);
                IsReads.State = Bootloader.IsEnable(BOOT_HANDLER.IS_READS);
                IsWrites.State = Bootloader.IsEnable(BOOT_HANDLER.IS_WRITES);
                WriteComlite.State = Bootloader.IsEnable(BOOT_HANDLER.WRITE_COMPLITE);
                ReadComlite.State = Bootloader.IsEnable(BOOT_HANDLER.READ_COMLITE);
                EraseComlite.State = Bootloader.IsEnable(BOOT_HANDLER.ERASE_COMLITE);
                CrcRead.State = Bootloader.IsEnable(BOOT_HANDLER.CRC_READ);
                CrcCalculated.State = Bootloader.IsEnable(BOOT_HANDLER.CRC_CALCULATED);
                MainStartated.State = Bootloader.IsEnable(BOOT_HANDLER.MAIN_STARTED);
            }
        }
    }
}
