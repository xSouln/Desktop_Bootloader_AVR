using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xLib;
using xLibWpf.UI_Propertys;
using static Bootloader_AVR.Types;

namespace Bootloader_AVR.UI_Propertys
{
    public class UI_Options : NotifyPropertyChanged
    {
        private BootOptionsT options = new BootOptionsT();

        public UI_State WordSize = new UI_State { Name = "Word size" };
        public UI_State PageSize = new UI_State { Name = "Page size" };
        public UI_State PagesCount = new UI_State { Name = "Pages count" };

        public BootOptionsT Value
        {
            set
            {
                options = value;
                WordSize.Value = options.WordSize;
                PageSize.Value = options.PageSize;
                PagesCount.Value = options.PagesCount;
            }
            get { return options; }
        }
    }
}
