using System;

namespace TCPServer
{
    class Program
    {
        static ServerObject server; // сервер
        static void Main(string[] args)
        {
            try
            {
                server = new ServerObject();
                server.Listen();
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }
    }
}
