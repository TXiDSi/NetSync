using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Timers;
using EventHandler = System.EventHandler;

namespace DefaultServer
{
    public class NetManager
    {
        //监听Socket
        public static Socket listenfd;
        //客户端socket及其状态信息
        public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
        //Select的检查列表
        static List<Socket> checkRead = new List<Socket>();
        //ping间隔
        public static long pingInterval = 30;

        public static void StartLoop(int listenPort)
        {
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1"); 
            IPEndPoint ipe = new IPEndPoint(ipAdr, listenPort);
            listenfd.Bind(ipe);
            listenfd.Listen(20);
            Console.WriteLine("服务器已启动");
            while (true)
            {
                ResetCheckRead();
                Socket.Select(checkRead, null, null, 1000);
                for (int i = checkRead.Count - 1; i >= 0; i--)
                {
                    Socket clientfd = checkRead[i];
                    if (clientfd == listenfd)
                    {
                        ReadListenfd(listenfd);
                    }
                    else
                    {
                        ReadClientfd(clientfd);
                    }
                    
                }

                //Timer();
            }
        }

        public static void ResetCheckRead()
        {
            checkRead.Clear();
            checkRead.Add(listenfd);
            foreach (ClientState s in clients.Values)
            {
                checkRead.Add(s.socket);
            }
        }

        public static void ReadListenfd(Socket listenfd)
        {
            try
            {
                Socket clientfd = listenfd.Accept();
                Console.WriteLine("客户端连接成功");
                ClientState cs = new ClientState();
                cs.socket = clientfd;
                clients.Add(clientfd, cs);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }
        
        public static void ReadClientfd(Socket clientfd)
        {
            ClientState state = clients[clientfd];
            ByteArray readBuff = state.readBuff;
            
            int count = 0;

            if (readBuff.remain <= 50)
            {
                OnReceiveData(state);
                readBuff.MoveBytes();
            }

            if (readBuff.remain <= 0)
            {
                Console.WriteLine("客户端缓冲区无法释放");
                Close(state);
                return;
            }

            try
            {
                count = clientfd.Receive(readBuff.bytes, readBuff.writeIndex, readBuff.remain, 0);
                Console.WriteLine("客户端发送数据:" + count);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                Close(state);
                return;
            }

            if (count <= 0)
            {
                Console.WriteLine("客户端断开连接");
                Close(state);
                return;
            }
            
            readBuff.writeIndex += count;
            OnReceiveData(state);
            readBuff.CheckAndMoveBytes();
        }
        
        static void Timer()
        {
            MethodInfo mei = typeof(EventHandler).GetMethod("OnTimer");
            object[] ob = {};
            if(mei!=null)
            mei.Invoke( null, ob);
        }

        public static void OnReceiveData(ClientState state)
        {
            ByteArray readBuff = state.readBuff;
            byte[] bytes = readBuff.bytes;

            if (readBuff.length <=2)
            {
                return;
            }
            
            Int16 bodyLength = (Int16)((bytes[readBuff.readIndex+1] << 8) | bytes[readBuff.readIndex]);
            if (readBuff.length < bodyLength+2)
            {
                return;
            }
            readBuff.readIndex += 2;
            
            int nameCount = 0;
            string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIndex, out nameCount);
            
            if (protoName == "")
            {
                Console.WriteLine("协议名错误");
                Close(state);
            }
            readBuff.readIndex += nameCount;
            
            int bodyCount = bodyLength-nameCount;
            MsgBase msg = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIndex, bodyCount);
            readBuff.readIndex += bodyCount;
            readBuff.CheckAndMoveBytes();
            
            if (msg == null)
            {
                Console.WriteLine("消息体错误");
                return;
            }
            
            Console.WriteLine("收到消息:" + msg.protoName);
            Console.WriteLine();
            MethodInfo mei = typeof(MsgHandler).GetMethod(protoName);
            object[] ob = {state,msg};
            if (mei != null)
            {
                mei.Invoke(null, ob);
            }
            else
            {
                Console.WriteLine("协议名函数错误"+protoName);
            }

            if (readBuff.length > 2)
            {
                OnReceiveData(state);
            }
                
            
            
        }

        public static void Close(ClientState state)
        {
            MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
            object[] ob = { state };
            mei.Invoke(null, ob);
            
            state.socket.Close();
            Console.WriteLine("客户端断开连接");
            clients.Remove(state.socket);
        }

        public static void Send(ClientState state, MsgBase msg)
        {
            if (state == null)
            {
                Console.WriteLine("客户端断开连接");
                return;
            }

            if (!state.socket.Connected)
            {
                Console.WriteLine("客户端断开连接");
                return;
            }
              
            
            byte[] nameBytes = MsgBase.EncodeName(msg);
            byte[] bodyBytes = MsgBase.Encode(msg);
            int length = nameBytes.Length + bodyBytes.Length;
            byte[] sendBytes = new byte[length+2];
            
            sendBytes[0]=(byte)(length % 256);
            sendBytes[1]=(byte)(length / 256);
            Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
            Array.Copy(bodyBytes, 0, sendBytes, nameBytes.Length+2, bodyBytes.Length);

            try
            {
                int count = state.socket.Send(sendBytes);
                Console.WriteLine("向客户端发送消息长度:"+count);
                Console.WriteLine(state.socket.RemoteEndPoint+"发送数据:"+msg.protoName);
                Console.WriteLine();
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (long)ts.TotalMilliseconds;
        }
    }
}