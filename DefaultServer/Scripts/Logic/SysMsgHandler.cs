using System;

namespace DefaultServer
{

    public partial class SysMsgHandler
    {
        public static void MsgPing(ClientState state, MsgBase msg)
        {
            Console.WriteLine(msg.protoName);
        }
    }
}