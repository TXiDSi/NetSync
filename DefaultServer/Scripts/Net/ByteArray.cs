using System;

namespace DefaultServer
{
    public class ByteArray
    {
        public byte[] bytes;
        const int DEAFAULT_CAPACITY = 4096;
    
        public int readIndex = 0;
        public int writeIndex = 0;
        public int length{get{return writeIndex - readIndex;}}
        public int remain{get{return capacity - writeIndex;}}
        public int capacity = 0;
        public int initSize = 0;

        public ByteArray(byte[] bs)
        {
            bytes = bs;
            readIndex = 0;
            writeIndex = bs.Length;
            capacity = bs.Length;
            initSize = bs.Length;
        }

        public ByteArray(int size = DEAFAULT_CAPACITY)
        {
            bytes = new byte[size];
            capacity = size;
            initSize = size;
            readIndex = 0;
            writeIndex = 0;
        }

        public void MoveBytes()
        {
            if (readIndex > 0)
            {
                Array.Copy(bytes, readIndex, bytes, 0, length);
                writeIndex -= readIndex;
                readIndex = 0;
            }
        }

        public void CheckAndMoveBytes()
        {
            if (remain < 50)
            {
                MoveBytes();
            }
        }
    
    }
}