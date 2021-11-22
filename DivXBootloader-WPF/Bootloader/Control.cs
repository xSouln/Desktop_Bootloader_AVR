using DivXBootloader_WPF.UI_Propertys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using xLib;
using xLib.UI_Propertys;
using xLibWpf.UI_Propertys;
using static DivXBootloader_WPF.Types;

namespace DivXBootloader_WPF
{  
    public static class Bootloader
    {
        public static EventTracer Tracer;
        private static void trace(string note) { Tracer?.Invoke(note); xTracer.Message(note); }

        public static UI_Handler Handler = new UI_Handler();
        public static UI_Errors Errors = new UI_Errors();
        public static UI_Mode Mode = new UI_Mode();
        public static UI_Options Options = new UI_Options();
        public static UI_OperationsInfo Info = new UI_OperationsInfo();

        public static BootStateT state = new BootStateT();

        public static UI_State FirmwareCrc = new UI_State { Name = "This firmware crc", Value = 0 };

        public static byte[] FirmwareFile;
        public static string FirmwareText;
        public static string HexText;

        private static Timer request_timer;
        private static Timer request_update_timer;

        public static CommunicationsControl Communication = new CommunicationsControl();

        public static BootStateT State
        {
            get { return state; }
            set
            {
                state = value;
                Handler.Value = value.Handler;
                Errors.Value = value.Errors;
                Mode.Value = value.Mode;
                Options.Value = value.Options;
                Info.Value = value.Info;
            }
        }

        public static bool IsEnable(BOOT_HANDLER request) { return (state.Handler & request) == request; }
        public static bool IsEnable(BOOT_ERRORS request) { return (state.Errors & request) == request; }
        public static bool IsEnable(BOOT_MODE request) { return (state.Mode & request) == request; }

        public static void Init()
        {
            Callbacks.Init();

            Requests.Get.State.IsNotification = true;

            request_timer = new Timer(requst_handler, null, 5000, 100);
            request_update_timer = new Timer(xSupport.RequestThreadUI, (RequestUI)requst_update_handler, 6000, 1000);
            Loader.ThreadStart(6000, 100);
            Communication.StartControl(2000);
        }
        private static void requst_handler(object arg)
        {
            if (Requests.Try.StartBoot.Handler()) return;
            if (Requests.Try.StartMain.Handler()) return;
            if (Requests.Try.ResetHandler.Handler()) return;
            if (Requests.Try.WritePage.Handler()) return;
            if (Requests.Try.ResetError.Handler()) return;
            if (Requests.Try.ReadPage.Handler()) return;
            if (Requests.Try.Erase.Handler()) return;

            if (Requests.Get.State.Handler()) return;
            if (Requests.Get.CalculatedCrc.Handler()) return;
            if (Requests.Get.FirmwareCrc.Handler()) return;
        }
        private static void requst_update_handler(object arg)
        {
            if (Requests.Get.State.IsNotification) { Get.State(); }
            if (Requests.Get.CalculatedCrc.IsNotification) { Get.CalculatedCrc(); }
            if (Requests.Get.FirmwareCrc.IsNotification) { Get.FirmwareCrc(); }
        }
        public static void Dispose()
        {
            Loader.ThreadStop();
            request_timer.Dispose();
            request_update_timer.Dispose();
            Communication.StopControl();
        }

        private static unsafe ushort calculate_crc(void* obj, int obj_size)
        {
            ushort crc_value = 0;
            byte* data = (byte*)obj;
            for (int i = obj_size; i > 0; i--)
            {
                crc_value ^= *data++;
                for (byte j = 0; j < 8; j++)
                {
                    if ((crc_value & 0x01) > 0) crc_value = (ushort)((crc_value >> 1) ^ 0xA001);
                    else crc_value >>= 1;
                }
            }
            return crc_value;
        }

        public static class Loader
        {
            private static Timer timer;
            private static Thread thred;
            public static bool is_update = true;

            public static UI_LoaderHandler State = new UI_LoaderHandler();
            public static UI_State FirmwarePages = new UI_State { Name = "Pages full" };

            public static uint page_total;
            public static uint page_count;
            public static int firmware_pages;

            public static byte[] data;

            public static byte[] page_data;
            public static ushort page_data_crc;

            public static bool crc_is_write = false;

            public static void ThreadStart(int start_delay, int period) { thred = new Thread(handler); thred.Start(); timer = new Timer(state_update, 0, start_delay, period); }
            public static void ThreadStop() { thred.Abort(); timer.Dispose(); }
            public static void Update() { is_update = false; }            
            private static unsafe void state_update(object obj) { is_update = false; }
            private static unsafe void handler(object obj)
            {
                try
                {
                    while (true)
                    {
                        if (!is_update)
                        {
                            is_update = true;

                            if (State.Comliting)
                            {
                                crc_is_write = false;
                                State.IsEnable = false;
                                State.Enabling = false;
                                State.Comlite = false;
                                State.Comliting = false;
                            }

                            if (Handler.ReadComlite.State) { Try.ResetError(BOOT_ERRORS.READ); goto end; }
                            if (Errors.Crc.State) { Try.ResetError(BOOT_ERRORS.CRC); goto end; }

                            if (State.Enabling)
                            {
                                if (Handler.WriteComlite.State) { Try.ResetHandler(BOOT_HANDLER.WRITE_COMPLITE); goto end; }
                                if (Handler.CrcCalculated.State) { Try.ResetHandler(BOOT_HANDLER.CRC_CALCULATED); goto end; }
                                if (Handler.CrcRead.State) { Try.ResetHandler(BOOT_HANDLER.CRC_READ); goto end; }
                                if (Handler.EraseComlite.State) { Try.ResetHandler(BOOT_HANDLER.ERASE_COMLITE); goto end; }
                                if (Errors.Crc.State) { Try.ResetError(BOOT_ERRORS.CRC); goto end; }

                                page_total = 0;
                                for (int i = 0; i < Options.Value.PageSize; i++) { page_data[i] = data[i]; }
                                fixed (byte* ptr = page_data) { page_data_crc = calculate_crc(ptr, Options.Value.PageSize); }

                                State.Enabling = false;
                                State.Comlite = false;
                                crc_is_write = false;
                                State.IsEnable = true;
                            }

                            if (State.IsEnable)
                            {
                                if (page_total < firmware_pages)
                                {
                                    if (page_total == Info.Value.LastWritePage && Handler.WriteComlite.State)
                                    {
                                        if (++page_total == firmware_pages) { page_total = (page_count - 1); goto end; }
                                        uint offset = page_total * Options.Value.PageSize;

                                        for (int i = 0; i < Options.Value.PageSize; i++) { page_data[i] = data[i + offset]; }
                                        fixed (byte* ptr = page_data) { page_data_crc = calculate_crc(ptr, Options.Value.PageSize); }
                                    }

                                    if (Handler.WriteComlite.State) { Try.ResetHandler(BOOT_HANDLER.WRITE_COMPLITE); goto end; }

                                    Try.WritePage((ushort)page_total, page_data, page_data_crc);                                    
                                    goto end;
                                }

                                if (Handler.WriteComlite.State) { Try.ResetHandler(BOOT_HANDLER.WRITE_COMPLITE); goto end; }

                                if (page_total != Info.Value.LastWritePage)
                                {
                                    uint offset = page_total * Options.Value.PageSize;

                                    for (int i = 0; i < Options.Value.PageSize; i++) { page_data[i] = data[i + offset]; }
                                    fixed (byte* ptr = page_data) { page_data_crc = calculate_crc(ptr, Options.Value.PageSize); }

                                    Try.WritePage((ushort)page_total, page_data, page_data_crc);
                                }

                                State.IsEnable = false;
                                State.Comlite = true;
                            }

                            if (State.Comlite)
                            {
                                if (!Handler.CrcRead.State) { Get.FirmwareCrc(); goto end; }
                                if (!Handler.CrcCalculated.State) { Get.CalculatedCrc(); goto end; }                                
                            }
                        }
                        end:;
                    }
                }
                catch(Exception e)
                {
                    State.IsEnable = false;
                    State.Enabling = false;
                    State.Comlite = false;
                    State.Comliting = false;

                    xTracer.Message(e.ToString());
                }
            }
        }

        public static class Get
        {
            public static void State() { Requests.Get.State.Prepare(); }
            public static void CalculatedCrc() { Requests.Get.CalculatedCrc.Prepare(); }
            public static void FirmwareCrc() { Requests.Get.FirmwareCrc.Prepare(); }
        }

        public static class Try
        {
            public static void StartMain() { Requests.Try.StartMain.Prepare(); }
            public static void StartBoot() { Requests.Try.StartBoot.Prepare(); }
            public static unsafe void ReadPage(ushort number) { Requests.Try.ReadPage.Prepare(&number); }
            public static unsafe void ResetHandler(BOOT_HANDLER request) { Requests.Try.ResetHandler.Prepare(&request); }
            public static unsafe void ResetError(BOOT_ERRORS request) { Requests.Try.ResetError.Prepare(&request); }
            public static void Erase() { Requests.Try.Erase.Prepare(); }
            public static unsafe void WritePage(ushort page, byte[] data, ushort crc)
            {
                if(data == null || data.Length == 0) { return; }
                byte[] request = new byte[sizeof(ushort) + data.Length + sizeof(ushort)];

                xCBase.MemCopy(request, &page, sizeof(ushort), 0);
                xCBase.MemCopy(request, data, sizeof(ushort));
                xCBase.MemCopy(request, &crc, sizeof(ushort), data.Length + sizeof(ushort));

                Requests.Try.WritePage.Prepare(request);
            }
        }

        public static unsafe bool LoadFirmware()
        {
            if(FirmwareText != null && FirmwareText.Length > 0 && (Options.Value.PagesCount > 0) && (Options.Value.PageSize > 0))
            {
                Loader.page_total = 0;
                Loader.page_count = Options.Value.PagesCount;
                Loader.page_data = new byte[Options.Value.PageSize];

                Loader.data = new byte[Options.Value.PagesCount * Options.Value.PageSize];
                Loader.firmware_pages = xHexReader.GetBin(FirmwareText, Loader.data, Options.Value.PagesCount, Options.Value.PageSize) + 1;
                Loader.FirmwarePages.Value = Loader.firmware_pages;

                Requests.Try.WritePage = new xRequest(Requests.Try.Prefix, RESPONSES.BL_TRY_PROGRAMM_PAGE, sizeof(ushort) + Options.Value.PageSize + sizeof(ushort), Requests.END_PACKET);

                if (Loader.data != null)
                {
                    BootInfoT info = new BootInfoT();
                    fixed (byte* ptr = Loader.data)
                    {
                        info.Crc = calculate_crc(ptr, Loader.firmware_pages * Options.Value.PageSize);
                        info.PagesFull = (ushort)Loader.firmware_pages;
                        xCBase.MemCopy(ptr + Loader.data.Length - sizeof(BootInfoT), &info, sizeof(BootInfoT), 0);                        
                    }
                    FirmwareCrc.Value = info.Crc;
                    Loader.State.Enabling = true;
                    return true;
                }
                else { Loader.State.DataIsSelected = false; }                
                return true;
            }
            return false;
        }

        public static unsafe bool OpenFile(string file_path)
        {
            FileInfo file_info;
            if (file_path != null)
            {
                file_info = new FileInfo(file_path);
                FirmwareFile = new byte[file_info.Length];
                using (FileStream Stream = new FileStream(file_path, FileMode.Open))
                {
                    Stream.Read(FirmwareFile, 0, (int)file_info.Length);

                    HexText = Encoding.UTF8.GetString(FirmwareFile);
                    FirmwareText = xHexReader.GetText(HexText, null);
                    if(FirmwareText == null || FirmwareText.Length == 0) { return false; }
                    Loader.State.DataIsSelected = true;
                    return true;
                }
            }
            return false;
        }

        public static void StopLoadFirmware()
        {
            Loader.State.IsEnable = false;
            Loader.State.Enabling = false;
            Loader.State.Comlite = false;
            Loader.State.Comliting = false;
        }
        public static unsafe void DataReceiver(xReceiverData ReceiverData)
        {
            string convert = "";
            string temp = "";
            ResponseT* response = (ResponseT*)ReceiverData.Content;
            int obj_size;

            if (response->Prefix.Start == Responses.START_CHARECTER)
            {
                if (ReceiverData.Size < sizeof(ResponseT)) { ReceiverData.xRx.Response = xRxResponse.Storage; return; } //trace("Receiver: Storage");
                obj_size = ReceiverData.Size - sizeof(ResponseT);

                if (obj_size < response->Info.Size) { ReceiverData.xRx.Response = xRxResponse.Storage; return; }
                if (obj_size > response->Info.Size) { trace("error content size"); goto error; }

                if (xCBase.Identification(Callbacks.Commands, ReceiverData.Content, ReceiverData.Size)) { Communication.Update(); goto end; };
            }
            error:
            convert = xConverter.ToStrHex(ReceiverData.xRx.Buf.Data, ReceiverData.Size);
            try { temp = xConverter.GetString(ReceiverData.Content, ReceiverData.Size); }
            catch { temp = "Encoding error"; }
            trace("Trace: " + temp + " {" + convert + "}");

            end: ReceiverData.xRx.Response = xRxResponse.Accept;
        }
    }
}
