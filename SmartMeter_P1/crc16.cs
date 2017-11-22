using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartMeter_P1
{
    class crc16
    {
        //DSMR v4.0 final P1.pdf
        // CRC is a CRC16 value calculated over the preceding characters in the data message.
        // (from / to ! using the polynomial: x^16 + x^15 + x^2 + 1 ).
        // The value is represented as 4 hexadecimal characters (MSB first).

        //INTERNET
        //http://stackoverflow.com/questions/22860356/how-to-generate-a-crc-16-from-c-sharp


        const ushort polynomial = 0xA001;
        static readonly ushort[] table = new ushort[256];

        public int calcChecksum(string text)
        {
            int crc = 0;
            int value;
            for (int i = 0; i < text.Length; i+=2)
            {
                value = (int)text[i];
                crc = crc ^ value;
                for (int y = 0; y < 8; y++)
                {
                    if ((crc & 0x0001) == 0x0001)
                    {
                        crc = (crc >> 1) ^ 0xA001;
                    }
                    else
                    {
                        crc = crc >> 1;
                    }
                }
            }

            string hexValue = string.Format("{0:X}", crc);
            
            return crc;
        }

        public ushort ComputeChecksum(byte[] bytes)
        {
            ushort crc = 0;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }
            return crc;
        }

        static crc16()
        {
            ushort value;
            ushort temp;
            for (ushort i = 0; i < table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                table[i] = value;
            }
        }

        public bool Check(string text, string checksum)
        {
            bool check = false;

            byte[] bytesText = GetBytes(text);

            ushort computedchecksum = ComputeChecksum(bytesText);

            string hexValue = string.Format("{0:X}", computedchecksum);

            return check;
        }

        //string to bytes
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        //string to bytes
        static byte[] GetBytesORG(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

    }
}