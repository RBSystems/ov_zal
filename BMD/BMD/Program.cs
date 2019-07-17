using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    public struct Received
    {
        public IPEndPoint Sender;
        public byte[] Message;
    }
    class UdpUser
    {
        private UdpClient Client;

        private UdpUser()
        {
            Client = new UdpClient();
        }

        public static UdpUser ConnectTo(string hostname, int port)
        {
            var connection = new UdpUser();
            connection.Client.Connect(hostname, port);
            return connection;
        }

        public void Send(byte[] message)
        {
            Client.Send(message, message.Length);
        }
        public async Task<Received> Receive()
        {
            var result = await Client.ReceiveAsync();
            return new Received()
            {
                Message = result.Buffer,
                Sender = result.RemoteEndPoint
            };
        }

    }

    class UdpListener
    {
        private IPEndPoint _listenOn;
        private UdpClient Client;

        public UdpListener() : this(new IPEndPoint(IPAddress.Any, 4321))
        {
        }

        public UdpListener(IPEndPoint endpoint)
        {
            _listenOn = endpoint;
            Client = new UdpClient(_listenOn);
        }
        public async Task<Received> Receive()
        {
            var result = await Client.ReceiveAsync();
            return new Received()
            {
                Message = result.Buffer,
                Sender = result.RemoteEndPoint
            };
        }

    }


    class Program
    {
        static void Main(string[] args)
        {
            bool isInit = true;
            var sessionID = new byte[] { 0x00, 0x29 };
            var remotePacketID = new byte[] { 0x00, 0x00 };
            var localPacketID = new byte[] { 0x00, 0x00 };

            var client = UdpUser.ConnectTo("192.168.0.50", 9910);

            var server = new UdpListener();

            Task.Factory.StartNew(async () => {
                while (true)
                {
                    var received = await server.Receive();
                    var read = Encoding.ASCII.GetString(received.Message, 0, received.Message.Length);
                    var datagramSendCmd = new byte[]
                                 { 0x08,0x18,0x00,0x00,
                                   0x00,0x00,0x00,0x00,
                                   0x00,0x00,0x00,0x00,
                                   0x00,0x0c,0x8f,0x00,
                                   0x43,0x50,0x67,0x49,
                                   0x00,0xda,0x00,0x00 };
                    datagramSendCmd[2] = sessionID[0];
                    datagramSendCmd[3] = sessionID[1];
                    datagramSendCmd[4] = remotePacketID[1];
                    datagramSendCmd[5] = remotePacketID[0];
                    int LPID = BitConverter.ToUInt16(localPacketID, 0);
                    if (!isInit)
                    {
                        localPacketID = BitConverter.GetBytes(++LPID);
                        datagramSendCmd[10] = localPacketID[1];
                        datagramSendCmd[11] = localPacketID[0];
                    }
                    switch (read)
                    {
                        case "init":
                            var datagramSend = new byte[]
                                { 0x10,0x14,0x00,0x29,
                                  0x00,0x00,0x00,0x00,
                                  0x00,0xab,0x00,0x00,
                                  0x01,0x00,0x00,0x00,
                                  0x00,0x00,0x00,0x00 };
                            isInit = true;
                            client.Send(datagramSend);
                            break;
                        case "c1":
                            datagramSendCmd[23] = 0x01;
                            client.Send(datagramSendCmd);
                            Console.WriteLine("Cam1 send. Pong {0}, Ping {1}", BitConverter.ToUInt16(remotePacketID, 0), LPID);
                            break;
                        case "c2":
                            datagramSendCmd[23] = 0x02;
                            client.Send(datagramSendCmd);
                            Console.WriteLine("Cam2 send. Pong {0}, Ping {1}", BitConverter.ToUInt16(remotePacketID, 0), LPID);
                            break;
                        case "c3":
                            datagramSendCmd[23] = 0x03;
                            client.Send(datagramSendCmd);
                            Console.WriteLine("Cam3 send. Pong {0}, Ping {1}", BitConverter.ToUInt16(remotePacketID, 0), LPID);
                            break;
                        case "c4":
                            datagramSendCmd[23] = 0x04;
                            client.Send(datagramSendCmd);
                            Console.WriteLine("Cam4 send. Pong {0}, Ping {1}", BitConverter.ToUInt16(remotePacketID, 0), LPID);
                            break;
                        case "c5":
                            datagramSendCmd[23] = 0x05;
                            client.Send(datagramSendCmd);
                            Console.WriteLine("Cam5 send. Pong {0}, Ping {1}", BitConverter.ToUInt16(remotePacketID, 0), LPID);
                            break;
                        case "c6":
                            datagramSendCmd[23] = 0x06;
                            client.Send(datagramSendCmd);
                            Console.WriteLine("Cam6 send. Pong {0}, Ping {1}", BitConverter.ToUInt16(remotePacketID, 0), LPID);
                            break;
                        case "ping":
                            if (!isInit)
                            {
                                var datagramSendping = new byte[]
                                {0x08,0x0c,0x00,0x00,
                                 0x00,0x00,0x00,0x00,
                                 0x00,0x00,0x00,0x00};
                                datagramSendping[2] = sessionID[0];
                                datagramSendping[3] = sessionID[1];
                                datagramSendping[10] = localPacketID[1];
                                datagramSendping[11] = localPacketID[0];
                                client.Send(datagramSendping);
                                Console.WriteLine("Ping {0}", LPID);
                            }
                            break;
                        default:
                            Console.WriteLine("Unknown command");
                            break;
                    }
                }
            });


            Task.Factory.StartNew(async () => {
                while (true)
                {
                    try
                    {
                        var received = await client.Receive();
                        if (isInit)
                        {
                            if ((received.Message[2] == 0x00) && (received.Message[3] == 0x29))
                            {
                                var datagramSend = new byte[]
                                { 0x80,0x0c,0x00,0x29,
                                  0x00,0x00,0x00,0x00,
                                  0x00,0x18,0x00,0x00 };
                                client.Send(datagramSend);
                                Console.WriteLine("Init received");
                            }
                            else
                            {
                                sessionID[0] = received.Message[2];
                                sessionID[1] = received.Message[3];
                                Console.WriteLine("Session ID obtained");
                                isInit = false;
                            }
                        }
                        else
                        {
                            if (received.Message[9] == 0x00)
                            {
                                remotePacketID[1] = received.Message[10];
                                remotePacketID[0] = received.Message[11];
                            }
                            if (received.Message.Length < 215)
                            {
                                var datagramSend = new byte[]
                                {0x80,0x0c,0x00,0x00,
                                 0x00,0x00,0x00,0x00,
                                 0x00,0x00,0x00,0x00};
                                datagramSend[2] = sessionID[0];
                                datagramSend[3] = sessionID[1];
                                datagramSend[4] = remotePacketID[1];
                                datagramSend[5] = remotePacketID[0];
                                datagramSend[9] = 0xa2;
                                client.Send(datagramSend);
                                Console.WriteLine("Pong {0}", BitConverter.ToUInt16(remotePacketID, 0));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex);
                    }
                }
            });


            string conread;
            do
            {
                conread = Console.ReadLine();
            } while (conread != "q");
        }

    }
}
