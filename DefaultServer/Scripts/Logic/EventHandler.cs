using System;

namespace DefaultServer
{
    public class EventHandler
    {
        public static void OnDisconnect(ClientState state)
        {
            MsgLeave msgLeave = new MsgLeave();
            msgLeave.desc = state.socket.RemoteEndPoint.ToString();
            foreach (ClientState cState in NetManager.clients.Values)
            {
                NetManager.Send(cState,msgLeave);
            }
        }
        public static void OnTimer()
        {
            Console.WriteLine("Timer");
        }
    }
}