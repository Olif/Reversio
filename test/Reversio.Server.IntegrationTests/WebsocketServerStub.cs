﻿using System;
using Reversio.WebSockets;

namespace Reversio.Server.IntegrationTests
{
    public class WebSocketServerStub : WebSocketServer
    {
        public Action<IWebSocketConnection, string> MessageReceived;

        public Action<IWebSocketConnection> ConnectionClosed;

        public Action<IWebSocketConnection> ConnectionOpened;

        public WebSocketServerStub()
        {
            MessageReceived = (conn, msg) => { };
            ConnectionClosed = (conn) => { };
            ConnectionOpened = (conn) => { };
        }

        public override void OnMessageReceived(IWebSocketConnection conn, string message)
        {
            MessageReceived(conn, message);
        }

        public override void OnConnectionClosed(IWebSocketConnection conn)
        {
            ConnectionClosed(conn);
        }

        public override void OnConnectionOpened(IWebSocketConnection conn)
        {
            ConnectionOpened(conn);
        }
    }
}