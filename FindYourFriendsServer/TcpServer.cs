﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using SharedCodePortable;
using SharedCodePortable.Packets;

namespace FindYourFriendsServer
{
    internal class TcpServer
    {
        private static readonly int Port = 1337;
        private static TcpListener Listener;

        public static void Start()
        {
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();

            while (true)
            {
                var client = Listener.AcceptTcpClient();

                new Thread(HandleConnection).Start(client);
            }
        }

        private static void HandleConnection(object o)
        {
            var client = (TcpClient) o;

            Console.WriteLine($"Connection from: {client.Client.RemoteEndPoint}");

            var buffer = new byte[81920];

            var bytesRead = client.GetStream().Read(buffer, 0, buffer.Length);

            var jsonBytes = buffer.Take(bytesRead).ToArray();

            var p = JsonConvert.DeserializeObject<Packet>(Encoding.Default.GetString(jsonBytes));

            Console.WriteLine($"Request: {p.PacketType}");

            switch (p.PacketType)
            {
                case EPacketType.LoginRequest:
                    var request = JsonConvert.DeserializeObject<LoginRequest>(p.Payload);
                    Console.WriteLine($"Login request from {request.username} with password {request.password}");
                    var resp = new LoginResponse()
                    {
                        succes = true,
                        token = "xddsorandom"
                    };

                    Send(client, new Packet {PacketType = EPacketType.LoginResponse, Payload = JsonConvert.SerializeObject(resp)});
                    
                    break;

                case EPacketType.NewAccountRequest:

                    break;


                case EPacketType.RefreshRequest:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public static void Send(TcpClient client, Packet p)
        {
            Console.WriteLine($"Sending packet with type {p.PacketType}");
            var bytes = Encoding.Default.GetBytes(JsonConvert.SerializeObject(p));
            client.GetStream().Write(bytes, 0, bytes.Length);
            client.Close();
        }
    }
}