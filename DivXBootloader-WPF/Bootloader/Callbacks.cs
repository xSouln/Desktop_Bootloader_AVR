using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xLib;
using static DivXBootloader_WPF.Types;

namespace DivXBootloader_WPF
{
    public static class Callbacks
    {
        public static List<xCommand> Commands;
        public static unsafe void Init()
        {
            Commands = new List<xCommand>();

            xCBase.Add(Commands, Responses.Get.State, ResponseGetStates);
        }

        private static unsafe void ResponseGetStates(xCBaseCallback Response, CBASE_KEY Key)
        {
            if (Key == CBASE_KEY.ACCEPT)
            {
                Bootloader.State = *((BootStateT*)Response.Content);
                Bootloader.Loader.Update();
            }
        }
    }
}
