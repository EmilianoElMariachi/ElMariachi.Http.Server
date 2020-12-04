﻿using System.Net;
using System.Net.Sockets;

namespace ElMariachi.Http.Server
{
    public class TcpListenerEx : TcpListener
    {
        public TcpListenerEx(IPAddress ipAddress, in int port) : base(ipAddress, port)
        {
        }

        public bool IsListening => Active;
    }
}