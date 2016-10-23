using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWTag.Utils
{
    public static class ByteConverter
    {
        public static string GetString(byte[] Bytes, Encoding Encode)
        {
            return Encode.GetString(Bytes, 0, Bytes.Length);
        }

        public static string GetString(System.IO.BinaryReader Reader, int Length, Encoding Encode)
        {
            byte[] buf = new byte[Length];
            Reader.Read(buf, 0, Length);
            return GetString(buf, Encode);
        }

        public static byte[] GetBytes(string Text, Encoding Encode)
        {
            if (string.IsNullOrEmpty(Text)) return new byte[0];
            return Encode.GetBytes(Text);
        }

        public static byte[] GetFilledBytes(string Text, Encoding Encode, byte ByteToFill, int Length)
        {
            byte[] bytes = new byte[Length];
            byte[] str = GetBytes(Text, Encode);
            for(int i = 0;Length > i; i++)
            {
                if (i <= str.Length - 1)
                    bytes[i] = str[i];
                else
                    bytes[i] = ByteToFill;
            }

            return bytes;
        }

        public static int GetIntFromSynchsafe(byte[] Synchsafe)
        {
            return (Synchsafe[0] << 21) + (Synchsafe[1] << 14) + (Synchsafe[2] << 7) + Synchsafe[3];
        }

        public static int GetIntFromHexadecimal(byte[] Bytes)
        {
            return Convert.ToInt32(BitConverter.ToString(Bytes).Replace("-", string.Empty), 16);
        }

        public static byte[] GetSynchsafeBytes(int value)
        {
            byte[] result = new byte[4];
            result[0] = (byte)((value & 0xFE00000) >> 21);
            result[1] = (byte)((value & 0x01FC000) >> 14);
            result[2] = (byte)((value & 0x0003F80) >> 7);
            result[3] = (byte)((value & 0x000007F));

            return result;
        }

        public static int GetIntFromHexadecimal(System.IO.BinaryReader Reader, int Length)
        {
            byte[] buf = new byte[Length];
            Reader.Read(buf, 0, Length);
            return GetIntFromHexadecimal(buf);
        }

        public static byte[] ZeroPadding(byte[] Bytes, int Length, bool Left)
        {
            if (Bytes.Length >= Length)
            {
                if (Left) Array.Reverse(Bytes);
                return Bytes;
            }

            byte[] ret = new byte[Length];
            for(int i = 0;Length > i; i++)
            {
                if (Bytes.Length > i) ret[i] = Bytes[i];
                else ret[i] = 0;
            }

            if (Left) Array.Reverse(ret);
            return ret;
        }

        public static byte[] RemoveZero(byte[] Bytes)
        {
            List<byte> ret = new List<byte>();
            for(int i = 0;Bytes.Length > i; i++)
            {
                if (Bytes[i] != 0)
                    ret.Add(Bytes[i]);
            }
            return ret.ToArray();
        }

        public static byte[] GetBytesToReachNull(System.IO.BinaryReader Reader, bool RemoveNull)
        {
            List<byte> bytes = new List<byte>(30);
            while(true)
            {
                byte b = Reader.ReadByte();
                if (RemoveNull && b == 0) break;
                bytes.Add(b);
                if (b == 0) break;
            }

            return bytes.ToArray();
        }
    }
}
