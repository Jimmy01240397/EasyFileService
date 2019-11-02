using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityNetwork;
using UnityNetwork.Server;

namespace Servers
{
    public class Appllication : AppllicationTCPBase
    {
        public override PeerTCPBase AddPeerBase(TcpClient _peer, NetTCPServer server)
        {
            return new Peer(_peer, server, this);
        }

        public override void Setup()
        {

        }

        public override int GetPort()
        {
            return 4444;
        }

        public override void CleanUp()
        {

        }

        public override void TearDown()
        {

        }

        public override string UpdateData()
        {
            return base.UpdateData();
        }
    }
}
