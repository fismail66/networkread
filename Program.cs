using System;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace networkread
{
    class Program
    {
        static TcpChannelCT channel {get; set;}
        static void Main(string[] args)
        {
            try
            {
               //LogHelper.Log(LogTarget.File, "Hello");
                Console.WriteLine("Hello World!");
                //initAndRead().GetAwaiter().GetResult();
                Task.Run(async () => await initAndRead());
                int ss = Console.Read();
                channel.Disconnect();
                Console.WriteLine("disconnected requested");
                Console.Read();

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.Read();
            }
        }


        static async Task initAndRead()
        {
            Int32 port = 13000;
            string ServerIp ="127.0.0.1";
            
            channel = new TcpChannelCT();
            channel.ServerIp = ServerIp;
            channel.TcpPort = port;

            await channel.ConnectAsyncCH();

        }
    }
}
