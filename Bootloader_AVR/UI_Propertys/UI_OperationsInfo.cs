using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xLibWpf.UI_Propertys;
using static Bootloader_AVR.Types;

namespace Bootloader_AVR.UI_Propertys
{
    public class UI_OperationsInfo
    {
        private BootOperationInfoT operations_info = new BootOperationInfoT();

        public UI_State LastReadPage = new UI_State { Name = "Last read page" };
        public UI_State LastWritePage = new UI_State { Name = "Last write page" };
        public UI_State CalculatedCrc = new UI_State { Name = "Calculated crc" };
        public UI_State ReadsPagesFull = new UI_State { Name = "Reads pages full" };
        public UI_State ReadsFirmwareCrc = new UI_State { Name = "Reads firmware crc" };

        public BootOperationInfoT Value
        {
            set
            {
                operations_info = value;
                LastReadPage.Value = operations_info.LastReadPage;
                LastWritePage.Value = operations_info.LastWritePage;
                CalculatedCrc.Value = operations_info.CalculatedCrc;
                ReadsFirmwareCrc.Value = operations_info.Flash.Crc;
                ReadsPagesFull.Value = operations_info.Flash.PagesFull;
            }
            get { return operations_info; }
        }
    }
}
