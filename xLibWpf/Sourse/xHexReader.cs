using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xLib
{
    public static class xHexReader
    {
        public const int CHECKSUM_SIZE = 2;
        public const string END_ROW = "\r\n";
        public const int PREFIX_SIZE = 9;
        public const byte START_CHARECTAR = (byte)':';

        public const string COMMAND_KEY = "00";
        public const int COMMAND_KEY_START_INDEX_0 = 7;
        public const int DATA_START_INDEX_0 = 9;
        public class xHExData
        {
            public byte[] Data;
            public List<string> Rows;
            public List<int> Addresses;
        }

        public static string GetText(string hex_content, string row_separator)
        {
            string rows = "";
            List<byte> data = new List<byte>();
            List<byte> data_out = new List<byte>();
            if (row_separator == null) row_separator = "";

            for (int i = 0; i < hex_content.Length; i++)
            {
                data.Add((byte)hex_content[i]);

                if (i > 2 && hex_content[i] == END_ROW[1] && hex_content[i - 1] == END_ROW[0])
                {
                    if(data.Count > CHECKSUM_SIZE + PREFIX_SIZE + END_ROW.Length)
                    {
                        if(data[0] == START_CHARECTAR && data[COMMAND_KEY_START_INDEX_0] == COMMAND_KEY[0] && data[COMMAND_KEY_START_INDEX_0 + 1] == COMMAND_KEY[1])
                        {
                            for (int j = DATA_START_INDEX_0; j < data.Count - CHECKSUM_SIZE - END_ROW.Length; j++) { data_out.Add(data[j]); }

                            data.RemoveRange(0, PREFIX_SIZE);
                            data.RemoveRange(data.Count - END_ROW.Length - CHECKSUM_SIZE, END_ROW.Length + CHECKSUM_SIZE);

                            rows += Encoding.UTF8.GetString(data.ToArray()) + row_separator;
                        }
                    }
                    data.Clear();
                }                
            }

            return rows;
        }

        public static int GetBin(string hex_content, byte[] flash, int pages_count, int page_size)
        {
            string character = "";
            int pages_full = 0;
            int byte_count = 0;
            int flash_size = pages_count * page_size;

            if ((hex_content != null) && ((hex_content.Length & 1) == 0) && (hex_content.Length / 2 < flash_size) )
            {
                for (int i = 0; i < flash.Length; i++) { flash[i] = 0xff; }

                for (int i = 0; i < hex_content.Length; i += 2)
                {
                    character = "" + hex_content[i] + hex_content[i + 1];
                    flash[i / 2] = Convert.ToByte(character, 16);
                    if (++byte_count == page_size) { byte_count = 0; pages_full++; }
                }
                if(byte_count > 0) { pages_full++; }
            }
            return pages_full;
        }
    }
}
