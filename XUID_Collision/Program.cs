using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace XUID_Collision
{
    class Program
    {
        public static bool _iw4m = false;
        static void Main(string[] args)
        {
            Log(args[0]);
        }

        private static StreamWriter _fileStream;
        private static StreamWriter _ipStream;
        private static StreamWriter _xuidStream;
        private static Queue _players;
        private static Random rand;
        private static bool _done;
        static void Log(string server, int playlist = 504)
        {
            var servers = new List<string> {server, server};
            rand = new Random();
            ThreadPool.SetMaxThreads(20, 1000);
            var count = 0;
            foreach (var serv in servers)
            {
                ThreadPool.QueueUserWorkItem(ConnectServer, serv + ":" + playlist + ":" + count);
                count++;
            }
            while (true)
            {
                Thread.Sleep(100);
            }
            Console.Read();
        }

        static void ConnectServer(object serv)
        {
            var addstr = (string)serv;
            Console.WriteLine("{0}: Connecting to server", addstr);
            var server = addstr.Split(':')[0];
            var port = int.Parse(addstr.Split(':')[1]);
            var playlist = int.Parse(addstr.Split(':')[2]);
            var client = new UdpClient(server, port) { Client = { ReceiveTimeout = 10000 } };
            short clientport = 28960;
            long xuid = 76561201295905685;
            var partyjoin = new PartyJoinPacket(xuid, "a", playlist).Packet;
            Console.WriteLine("{0}: Sending partyjoin packet", addstr);
            client.Send(partyjoin, partyjoin.Length);
            var EP = new IPEndPoint(IPAddress.Any, 0);
            var fail = false;
            while (true)
            {
                try
                {
                    var bytes = client.Receive(ref EP);
                    var str = Encoding.ASCII.GetString(bytes);
                    if (str.Contains("0password"))
                    {
                        Console.WriteLine("{0}: Server requires password", addstr);
                        break;
                    }
                    else if (str.Contains("0invalidpassword"))
                    {
                        Console.WriteLine("{0}: Sent server an invalid password", addstr);
                        break;
                    }
                    else if (str.Contains("0dis"))
                    {
                        Console.WriteLine("{0}: Disconnected from server", addstr);
                        break;
                    }
                    else if (str.Contains("0partyaccept"))
                    {
                        Console.WriteLine("{0}: Received partyaccept", addstr);
                        if (str.Contains("0partyJoinFailed"))
                        {
                            Console.WriteLine("{0}: Recieved partyJoinFailed", addstr);
                            break;
                        }
                        var challenge = new PartyAcceptPacket(bytes).Challenge;
                        var memberjoin = new MemberJoinPacket(challenge, xuid, "", clientport).Packet;
                        Console.WriteLine("{0}: Sending server memberJoin and RAck packets", addstr);
                        client.Send(memberjoin, memberjoin.Length);
                        client.Send(RAckPacket.Packet, RAckPacket.Packet.Length);
                        _done = true;
                    }
                    else if (str.Contains("0partystate"))
                    {
                        Console.WriteLine("{0}: Received partystate", addstr);
                        break;
                    }
                    else if (str.Contains("0playlistIsOld"))
                    {
                        Console.WriteLine("{0}: Our playlist is old, rejected", addstr);
                        break;
                    }
                    else if (str.Contains("0rejoin"))
                    {
                        Console.WriteLine("{0}: Server is asking us to rejoin, disconnecting", addstr);
                        break;
                    }
                    else if (str.Contains("0playlistIsNew"))
                    {
                        Console.WriteLine("{0}: Our playlist is too new, rejected", addstr);
                        break;
                    }
                    else if (str.Contains("0partyFull"))
                    {
                        Console.WriteLine("{0}: Server is full, disconnecting", addstr);
                        break;
                    }
                    else if (str.Contains("0partyJoinFailed"))
                    {
                        Console.WriteLine("{0}: Failed to join party, disconnecting", addstr);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("{0}: Received unknown packet: {1}", addstr, str);
                    }
                }
                catch (SocketException e)
                {

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
