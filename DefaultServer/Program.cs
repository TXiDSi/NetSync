

namespace DefaultServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            NetManager.StartLoop(8888);
        }
    }
}