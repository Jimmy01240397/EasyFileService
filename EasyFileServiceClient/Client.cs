using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.IO;

using JimmikerNetwork;
using JimmikerNetwork.Client;
using PacketType;

namespace EasyFileServiceClient
{
    public class Client : ClientListen
    {
        public bool stop { get; set; } = false;
        public bool finish { get; set; } = false;

        public string downloadpath { get; set; } = "";

        public ClientLinker clientLinker { get; private set; }
        public Client(ProtocolType protocol)
        {
            clientLinker = new ClientLinker(this, protocol);
        }

        public bool Connect(string host, int port)
        {
            bool on = clientLinker.Connect(host, port);
            clientLinker.RunUpdateThread();
            return on;
        }

        public void DebugReturn(string message)
        {
            
        }

        public void OnEvent(SendData sendData)
        {
            
        }

        public void OnOperationResponse(SendData sendData)
        {
            switch((ResponseType)sendData.Code)
            {
                case ResponseType.listback:
                    {
                        string[] allpath = (string[])sendData.Parameters;
                        for(int i = 0; i < allpath.Length; i++)
                        {
                            Console.WriteLine(allpath[i]);
                        }
                        finish = true;
                        break;
                    }
                case ResponseType.downloadback:
                    {
                        switch ((DownloadReturnCode)sendData.ReturnCode)
                        {
                            case DownloadReturnCode.mkdir:
                                {
                                    if(!Directory.Exists(downloadpath + sendData.Parameters.ToString()))
                                    {
                                        Directory.CreateDirectory(downloadpath + sendData.Parameters.ToString());
                                    }
                                    break;
                                }
                            case DownloadReturnCode.sendfile:
                                {
                                    object[] getdata = (object[])sendData.Parameters;
                                    if((bool)getdata[3]) Console.WriteLine(getdata[0].ToString() + " => " + downloadpath + getdata[1].ToString());
                                    using (FileStream file = File.Open(downloadpath + getdata[1].ToString(), (bool)getdata[3] ? FileMode.Create : FileMode.Append))
                                    {
                                        byte[] buffer = (byte[])getdata[2];
                                        file.Write(buffer, 0, buffer.Length);
                                        file.Close();
                                    }
                                    break;
                                }
                            case DownloadReturnCode.end:
                                {
                                    finish = true;
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        public void OnStatusChanged(LinkCobe connect)
        {
            switch (connect)
            {
                case LinkCobe.Connect:
                    {
                        //Console.WriteLine("connect Success");
                        break;
                    }
                case LinkCobe.Failed:
                    {
                        //Console.WriteLine("connect Failed");
                        clientLinker.StopUpdateThread();
                        stop = true;
                        break;
                    }
                case LinkCobe.Lost:
                    {
                        //Console.WriteLine("connect Lost");
                        clientLinker.StopUpdateThread();
                        stop = true;
                        break;
                    }
            }
        }

        public PeerForP2PBase P2PAddPeer(object _peer, object publicIP, INetClient client, bool NAT)
        {
            throw new NotImplementedException();
        }
    }
}