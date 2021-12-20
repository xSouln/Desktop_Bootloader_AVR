using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using xLib;
using xLib.Sourse;
using static Bootloader_AVR.Types;

namespace Bootloader_AVR
{
    public enum RESPONSES : ushort
    {
        BL_GET_STATE,
        BL_GET_FIRMWARE_CRC,
        BL_GET_CALCULATE_FIRMWARE_CRC,
        BL_TRY_PROGRAMM_PAGE,
        BL_TRY_START_MAIN,
        BL_TRY_START_BOOT,
        BL_TRY_RESET_HANDLER,
        BL_TRY_RESET_ERROR,
        BL_TRY_READ_PAGE,
        BL_TRY_ERASE,
        BL_SET_BOOT_MODE,
        BL_CONFIRMATION
    }

    public static unsafe class Responses
    {
        public const char START_CHARECTER = '#';
        public const char END_CHARECTER = ':';
        public const string END_PACKET = "\r";

        public const string RESPONSE = "BRES";
        public static string PREFIX_RESPONSE = "" + START_CHARECTER + RESPONSE + END_CHARECTER;

        public static unsafe class Get
        {
            public static xResponse State = new xResponse(PREFIX_RESPONSE, RESPONSES.BL_GET_STATE, sizeof(BootStateT));
        }
    }
}
