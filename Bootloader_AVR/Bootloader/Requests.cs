using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xLib;
using static Bootloader_AVR.Types;

namespace Bootloader_AVR
{
    public static unsafe class Requests
    {
        public const char REQUEST_START_CHARECTER = '#';
        public const char REQUEST_END_CHARECTER = ':';
        public const string END_PACKET = "\r";

        public static class Set
        {
            public const string REQUEST_SET = "BREQ";
            public static string Prefix = "" + REQUEST_START_CHARECTER + REQUEST_SET + REQUEST_END_CHARECTER;

            public static xRequest FirmwareCrc = new xRequest(Prefix, RESPONSES.BL_SET_BOOT_MODE, sizeof(BootRequstSetMode), END_PACKET);
        }

        public static class Get
        {
            public const string REQUEST_GET = "BREQ";
            public static string Prefix = "" + REQUEST_START_CHARECTER + REQUEST_GET + REQUEST_END_CHARECTER;

            public static xRequest State = new xRequest(Prefix, RESPONSES.BL_GET_STATE, 0, END_PACKET);
            public static xRequest FirmwareCrc = new xRequest(Prefix, RESPONSES.BL_GET_FIRMWARE_CRC, 0, END_PACKET);
            public static xRequest CalculatedCrc = new xRequest(Prefix, RESPONSES.BL_GET_CALCULATE_FIRMWARE_CRC, 0, END_PACKET);
        }

        public static class Try
        {
            public const string REQUEST_TRY = "BREQ";
            public static string Prefix = "" + REQUEST_START_CHARECTER + REQUEST_TRY + REQUEST_END_CHARECTER;

            public static xRequest WritePage = new xRequest();
            public static xRequest StartMain = new xRequest(Prefix, RESPONSES.BL_TRY_START_MAIN, 0, END_PACKET);
            public static xRequest StartBoot = new xRequest(Prefix, RESPONSES.BL_TRY_START_BOOT, 0, END_PACKET);
            public static xRequest ResetHandler = new xRequest(Prefix, RESPONSES.BL_TRY_RESET_HANDLER, sizeof(BOOT_HANDLER), END_PACKET);
            public static xRequest ResetError = new xRequest(Prefix, RESPONSES.BL_TRY_RESET_ERROR, sizeof(BOOT_ERRORS), END_PACKET);
            public static xRequest ReadPage = new xRequest(Prefix, RESPONSES.BL_TRY_READ_PAGE, sizeof(ushort), END_PACKET);
            public static xRequest Erase = new xRequest(Prefix, RESPONSES.BL_TRY_ERASE, 0, END_PACKET);
        }
    }
}
