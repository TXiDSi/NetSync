using System;
using System.Collections.Generic;

namespace DefaultServer
{
    public partial class MsgHandler
    {
        public static void MsgTest(ClientState state, MsgBase msg)
        {
            NetManager.Send(state,msg);
            Console.WriteLine("运行函数"+msg.protoName);
        }
        
        public static void MsgPing(ClientState state, MsgBase msg)
        {
            Console.WriteLine("收到Ping");
            state.lastPingTime = NetManager.GetTimeStamp();
            MsgPong msgPong = new MsgPong();
            NetManager.Send(state,msgPong);
        }
        
        public static void MsgMove(ClientState state, MsgBase msg)
        {
            MsgMove msgMove = (MsgMove)msg;
            state.x = msgMove.x;
            state.y = msgMove.y;
            state.z = msgMove.z;
            state.eulX = msgMove.eulX;
            state.eulY = msgMove.eulY;
            state.eulZ = msgMove.eulZ;
            msgMove.desc = state.socket.RemoteEndPoint.ToString();
            foreach (ClientState cState in NetManager.clients.Values)
            {
                NetManager.Send(cState,msgMove);
            }
        }
        
        public static void MsgEnter(ClientState state, MsgBase msg)
        {
            MsgEnter msgEnter = (MsgEnter)msg;
            msgEnter.desc = state.socket.RemoteEndPoint.ToString();
            state.x = msgEnter.x;
            state.y = msgEnter.y;
            state.z = msgEnter.z;
            state.eulX = msgEnter.eulX;
            state.eulY = msgEnter.eulY;
            state.eulZ = msgEnter.eulZ;
            
            
            foreach (ClientState cState in NetManager.clients.Values)
            {
                MsgEnter me = new MsgEnter();
                me.x = cState.x;
                me.y = cState.y;
                me.z = cState.z;
                me.eulX = cState.eulX;
                me.eulY = cState.eulY;
                me.eulZ = cState.eulZ;
                me.desc = cState.socket.RemoteEndPoint.ToString();
                NetManager.Send(cState,msgEnter);
                NetManager.Send(state,me);
            }
            
        }
        
    }
}