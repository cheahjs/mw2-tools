using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace XUID_Collision
{
    public class PartyJoinPacket
    {
        public byte[] Packet;

        public PartyJoinPacket(long xuid, string password = "", int playlist = 504)
        {
            using (var writer = new BinaryWriter(new MemoryStream()))
            {
                using (var reader = new BinaryReader(writer.BaseStream))
                {
                    writer.Write(new byte[] { 0xff, 0xff, 0xff, 0xff });
                    writer.Write(("2joinParty " + (Program._iw4m ? "61586 " : "144 ")).ToCharArray());
                    writer.Write(xuid.ToString("X16").ToLower().ToCharArray());
                    writer.Write((" 1 1 " + playlist + " 1 1 pw" + password + " 1 ").ToCharArray());
                    writer.Write((byte)0x00);
                    reader.BaseStream.Position = 0;
                    Packet = reader.ReadBytes((int)reader.BaseStream.Length);
                }
            }
        }
    }

    public class PartyAcceptPacket
    {
        public string Challenge;

        public PartyAcceptPacket(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    reader.ReadBytes(7);
                    if (new string(reader.ReadChars(12)) != "0partyaccept")
                        return;
                    reader.ReadByte();
                    var str = "";
                    var count = 0;
                    while (true)
                    {
                        if (count >= 10)
                            break;
                        var character = reader.ReadChar();
                        if (character == ' ')
                            break;
                        str += character;
                        count++;
                    }
                    Challenge = str;
                }
            }
        }
    }

    public class MemberJoinPacket
    {
        public byte[] Packet;

        public MemberJoinPacket(string challenge, long xuid, string nick, short port)
        {
            using (var writer = new BinaryWriter(new MemoryStream()))
            {
                using (var reader = new BinaryReader(writer.BaseStream))
                {
                    writer.Write(new byte[] { 0xff, 0xff, 0xff, 0xff });
                    writer.Write("0memberJoin ".ToCharArray());
                    writer.Write(challenge.ToCharArray());
                    writer.Write(' ');
                    writer.Write(xuid.ToString("X16").ToLower().ToCharArray());
                    writer.Write(" war".ToCharArray());
                    writer.Write((byte)0x00);
                    writer.Write(IPAddress.Parse("127.0.0.1").GetAddressBytes());
                    writer.Write(IPAddress.Parse("192.168.0.232").GetAddressBytes());
                    writer.Write((ushort)port);
                    writer.Write((ushort)port);
                    writer.Write(new byte[32]);
                    writer.Write(nick.ToCharArray());
                    writer.Write(new byte[]
                                     {
                                         0x00, 0x00, 0x01, 0x33, 0x3a, 0x01, 0x44, 0x00, 0x00, 0x00, 0xf1, 0x0a, 0x00
                                     });
                    reader.BaseStream.Position = 0;
                    Packet = reader.ReadBytes((int)reader.BaseStream.Length);

                }
            }
        }
    }

    public class PartyStatePacket
    {
        public List<Player> Players;

        public PartyStatePacket(byte[] packet)
        {
            Players = new List<Player>();
            using (var stream = new MemoryStream(packet))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var str = Encoding.ASCII.GetString(packet);
                    var index = str.IndexOf("mp_", StringComparison.Ordinal);
                    reader.BaseStream.Position = index;
                    //  Find the end of map name
                    while (reader.ReadByte() != 0x00)
                    {
                    }
                    //  Find the end of gametype
                    while (reader.ReadByte() != 0x00)
                    {
                    }
                    reader.ReadBytes(8);
                    while (true)
                    {
                        try
                        {
                            var player = new Player { Name = "" };
                            while (true)
                            {
                                var be = reader.ReadByte();
                                var by = new ASCIIEncoding().GetString(new[] { be }).ToCharArray()[0];
                                if (by != '\0')
                                    player.Name += by;
                                else
                                    break;
                            }
                            reader.ReadBytes(4);
                            player.XUID = reader.ReadInt64();
                            if (!player.XUID.ToString("X16").StartsWith("011000"))
                                break;
                            var internalip = new IPAddress(reader.ReadBytes(4));
                            var externalip = new IPAddress(reader.ReadBytes(4));
                            var intport = reader.ReadUInt16();
                            var extport = reader.ReadUInt16();
                            player.InternalIP = new IPEndPoint(internalip, intport);
                            player.ExternalIP = new IPEndPoint(externalip, extport);
                            Players.Add(player);
                            reader.ReadBytes(50);
                        }
                        catch (EndOfStreamException)
                        {
                            break;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    public class RAckPacket
    {
        public static readonly byte[] Packet = new byte[] { 0xff, 0xff, 0xff, 0xff, 0x52, 0x41, 0x20, 0x32, 0x32, 0x00 };
    }

    public class DisconnectPacket
    {
        public byte[] Packet;

        public DisconnectPacket(long xuid)
        {
            Packet = Encoding.ASCII.GetBytes("00000dis " + xuid.ToString("X16").ToLower());
            Packet[0] = 0xFF;
            Packet[1] = 0xFF;
            Packet[2] = 0xFF;
            Packet[3] = 0xFF;
        }
    }

    public class Player
    {
        public string Name;
        public IPEndPoint InternalIP;
        public IPEndPoint ExternalIP;
        public long XUID;
    }
}
