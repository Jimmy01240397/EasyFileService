﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using JimmikerNetwork;
using JimmikerNetwork.Server;
using System.IO;

namespace EasyFileService
{
    public class Appllication : AppllicationBase
    {
        public string rootpath { get; private set; } = "";
        public event Action<MessageType, string> GetMessage;

        public Appllication(string rootpath, ProtocolType protocol):base(IPAddress.IPv6Any, 6875, protocol)
        {
            if (rootpath[rootpath.Length - 1] != '\\') rootpath += "\\";
            this.rootpath = rootpath;
        }

        protected override void Setup()
        {
            RunUpdateThread();
        }

        protected override PeerBase AddPeerBase(object _peer, INetServer server)
        {
            return new Peer(_peer, server, this);
        }

        protected override void TearDown()
        {
            
        }

        protected override void DebugReturn(MessageType messageType, string msg)
        {
            GetMessage?.Invoke(messageType, msg);
        }
    }
}