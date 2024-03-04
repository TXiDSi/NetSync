using System;
using System.Diagnostics;
using System.Web.Script.Serialization;
    public class MsgBase
    {
        //协议名
        public string protoName = "";
    
        static JavaScriptSerializer JS = new JavaScriptSerializer();
        //编码
        public static byte[] Encode(MsgBase msg)
        {
            string s = JS.Serialize(msg);
            return System.Text.Encoding.UTF8.GetBytes(s);
        }
    
        //解码
        public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
        {
            string s = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
            Console.WriteLine(protoName);
            Console.WriteLine(Type.GetType(protoName).GetType());
            MsgBase msg = (MsgBase)JS.Deserialize(s,Type.GetType(protoName));
            return msg;
        }

        public static byte[] EncodeName(MsgBase msg)
        {
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msg.protoName);
            Int16 len = (Int16)nameBytes.Length;
            byte[] bytes = new byte[2 + len];
            bytes[0] = (byte)(len % 256);
            bytes[1] = (byte)(len / 256);
            Array.Copy(nameBytes, 0, bytes, 2, len);
            return bytes;
        }

        public static string DecodeName(byte[] bytes, int offset, out int count)
        {
            count = 0;
            if (offset + 2 > bytes.Length)
            {
                return "";
            }
            Int16 len = (Int16)((bytes[offset+1] << 8)|bytes[offset]);
            if (len <= 0)
            {
                return "";
            }
            if (offset + 2 + len > bytes.Length)
            {
                return "";
            }
            count = 2 + len;
            string name =  System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
            return name;
        }
    }
