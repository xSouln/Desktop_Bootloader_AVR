namespace DivXBootloader_WPF
{
    public class Types
    {
        public enum BOOT_HANDLER : ushort
        {
            IS_ENABLE = (1 << 0),
            IS_READS = (1 << 1),
            IS_WRITES = (1 << 2),
            WRITE_COMPLITE = (1 << 3),
            READ_COMLITE = (1 << 4),
            ERASE_COMLITE = (1 << 5),
            CRC_READ = (1 << 6),
            CRC_CALCULATED = (1 << 7),
            MAIN_STARTED = (1 << 8)
        }
        public enum BOOT_ERRORS : ushort
        {
            CRC = (1 << 0),
            WRITE = (1 << 1),
            READ = (1 << 2),
            PAGE = (1 << 3)
        }
        public enum BOOT_MODE : ushort
        {
            IGNORE_CRC = (1 << 0),
            BOOT_ENABLE = (1 << 1)
        }
        
        public struct BootOptionsT
        {
            public ushort WordSize;            
            public ushort PageSize;
            public ushort PagesCount;
        }
        public struct BootInfoT
        {
            public ushort PagesFull;
            public ushort Crc;
        }

        public struct BootOperationInfoT
        {
            public ushort LastReadPage;
            public ushort LastWritePage;
            public ushort CalculatedCrc;
            public BootInfoT Flash;
        }

        public struct BootRequstSetMode
        {
            public ushort Action;
            public BOOT_MODE Mode;
        }

        public struct BootStateT
        {
            public BOOT_HANDLER Handler;
            public BOOT_ERRORS Errors;
            public BOOT_MODE Mode;
            public BootOptionsT Options;
            public BootOperationInfoT Info;
        }
    }
}
