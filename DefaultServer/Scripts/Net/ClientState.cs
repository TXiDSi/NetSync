using System.Net.Sockets;

namespace DefaultServer
{
    public class ClientState
    {
        public Socket socket;
        public ByteArray readBuff = new ByteArray();
        public long lastPingTime = 0;
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float eulX = 0;
        public float eulY = 0;
        public float eulZ = 0;
    }
}