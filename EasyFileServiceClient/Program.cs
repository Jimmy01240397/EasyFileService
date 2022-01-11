using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using JimmikerNetwork;
using JimmikerNetwork.Client;
using PacketType;
using ConsoleCtrl;

namespace EasyFileServiceClient
{
    class Program
    {
        const int buffersize = 1024 * 1024 * 5;
        enum Command
        {
            help,
            list,
            upload,
            download,
            mkdir,
            delete,
            move
        }
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("try 'EasyFileServiceClient help' for more information");
                return;
            }
            Command command;
            string host = "";
            int port = -1;
            string remotepath = "";
            string localpath = "";

            try
            {
                command = (Command)Enum.Parse(typeof(Command), args[0]);
            }
            catch (Exception)
            {
                Console.WriteLine("try 'EasyFileServiceClient help' for more information");
                return;
            }

            for(int i = 1; i < args.Length; i++)
            {
                switch(args[i])
                {
                    case "-h":
                        {
                            i++;
                            host = args[i];
                            break;
                        }
                    case "-p":
                        {
                            i++;
                            port = Convert.ToInt32(args[i]);
                            break;
                        }
                    case "-d":
                        {
                            i++;
                            remotepath = args[i];
                            break;
                        }
                    case "-s":
                        {
                            i++;
                            localpath = args[i];
                            break;
                        }
                    case "--help":
                        {
                            switch (command)
                            {
                                case Command.list:
                                    {
                                        Console.WriteLine("list -h <remotehost> -p <remoteport> -d <remotepath>");
                                        return;
                                    }
                                case Command.upload:
                                    {
                                        Console.WriteLine("upload -h <remotehost> -p <remoteport> -s <localpath> -d <remotepath>");
                                        return;
                                    }
                                case Command.download:
                                    {
                                        Console.WriteLine("download -h <remotehost> -p <remoteport> -d <remotepath> -s <localpath>");
                                        return;
                                    }
                                case Command.mkdir:
                                    {
                                        Console.WriteLine("mkdir -h <remotehost> -p <remoteport> -d <remotepath and dirname>");
                                        return;
                                    }
                                case Command.delete:
                                    {
                                        Console.WriteLine("delete -h <remotehost> -p <remoteport> -d <remotepath>");
                                        return;
                                    }
                                case Command.move:
                                    {
                                        Console.WriteLine("move -h <remotehost> -p <remoteport> -s <remote souce path> -d <remote destination path>");
                                        return;
                                    }
                            }
                            break;
                        }
                }
            }
            if(command != Command.help && (host == "" || port == -1))
            {
                Console.WriteLine("please add your remote host and port.");
                return;
            }
            else if(command == Command.help)
            {
                Console.WriteLine(
                    "Usage: EasyFileServiceClient <help|list|upload|download|mkdir> [options...]\n" +
                    " --help   info for command\n" +
                    " -h   remote host\n" +
                    " -p   remote port\n" +
                    " -d   destination(remote) path\n" +
                    " -s   source(local) path\n"
                    );
                return;
            }


            Client client = new Client(ProtocolType.Tcp);
            client.Connect(host, port);

            ConsoleCtrl.ConsoleCtrl consoleCtrl = (Environment.OSVersion.Platform == PlatformID.Unix) || (Environment.OSVersion.Platform == PlatformID.MacOSX) || ((int)Environment.OSVersion.Platform == 128) ? (ConsoleCtrl.ConsoleCtrl)new UnixConsoleCtrl() : (ConsoleCtrl.ConsoleCtrl)new WinConsoleCtrl();

            consoleCtrl.OnExit += (sender, e) =>
            {
                Console.WriteLine("Closing...");
                client.clientLinker.Disconnect();
                SpinWait.SpinUntil(() => client.stop);
                Console.WriteLine("OK...");
            };

            SpinWait.SpinUntil(() => client.clientLinker.linkstate != LinkCobe.None);

            if (client.clientLinker.linkstate == LinkCobe.Connect)
            {
                switch(command)
                {
                    case Command.list:
                        {
                            client.clientLinker.Ask((byte)RequestType.list, remotepath);
                            break;
                        }
                    case Command.upload:
                        {
                            if(localpath == "")
                            {
                                Console.WriteLine("please add source(local) path when you use upload.");
                                client.finish = true;
                                break;
                            }
                            if(remotepath != "") if (remotepath[remotepath.Length - 1] != client.nextdir) remotepath += client.nextdir.ToString();
                            client.clientLinker.Ask((byte)RequestType.upload, new object[] { remotepath });
                            Uploader(client, localpath, remotepath);
                            client.finish = true;
                            break;
                        }
                    case Command.download:
                        {
                            if (localpath == "" || remotepath == "")
                            {
                                Console.WriteLine("please add source(local) and destination(remote) path when you use download.");
                                client.finish = true;
                                break;
                            }
                            if(!Directory.Exists(localpath))
                            {
                                Console.WriteLine("please use a exist directory in source(local) path.");
                                client.finish = true;
                                break;
                            }
                            if (localpath[localpath.Length - 1] != client.nextdir) localpath += client.nextdir.ToString();
                            try
                            {
                                client.downloadpath = Path.GetFullPath(localpath);
                            }
                            catch(Exception)
                            {
                                Console.WriteLine("bad source(local) path.");
                                client.finish = true;
                                break;
                            }
                            client.clientLinker.Ask((byte)RequestType.download, remotepath);
                            break;
                        }
                    case Command.mkdir:
                        {
                            if (remotepath == "")
                            {
                                Console.WriteLine("please add destination(remote) path when you use mkdir.");
                                client.finish = true;
                                break;
                            }
                            client.clientLinker.Ask((byte)RequestType.mkdir, remotepath);
                            client.finish = true;
                            break;
                        }
                    case Command.delete:
                        {
                            if (remotepath == "")
                            {
                                Console.WriteLine("please add destination(remote) path when you use delete.");
                                client.finish = true;
                                break;
                            }
                            client.clientLinker.Ask((byte)RequestType.delete, remotepath);
                            client.finish = true;
                            break;
                        }
                    case Command.move:
                        {
                            if (localpath == "" || remotepath == "")
                            {
                                Console.WriteLine("please add source and destination path when you use move.");
                                client.finish = true;
                                break;
                            }
                            client.clientLinker.Ask((byte)RequestType.move, new string[] { localpath, remotepath });
                            client.finish = true;
                            break;
                        }
                }
            }
            //Console.ReadKey();
            //Console.WriteLine("Closing...");
            //client.clientLinker.Disconnect();
            SpinWait.SpinUntil(() => client.finish);
            client.clientLinker.Disconnect();
            SpinWait.SpinUntil(() => client.stop);
            //Console.WriteLine("OK...");
        }

        static void Uploader(Client client, string path, string remotepath)
        {
            if (remotepath[remotepath.Length - 1] != client.nextdir) remotepath += client.nextdir.ToString();
            if (Directory.Exists(path))
            {
                client.clientLinker.Ask((byte)RequestType.mkdir, remotepath + Path.GetFileName(path));
                List<string> files = new List<string>(Directory.GetFiles(path));
                files.AddRange(Directory.GetDirectories(path));
                for (int i = 0; i < files.Count; i++)
                {
                    Uploader(client, files[i], remotepath + Path.GetFileName(path) + client.nextdir.ToString());
                }
            }
            else if(File.Exists(path))
            {
                Console.WriteLine(path + " => " + remotepath + Path.GetFileName(path));
                using (FileStream file = File.Open(path, FileMode.Open))
                {
                    byte[] buffer = new byte[buffersize];
                    bool isfirst = true;
                    for (int cont = file.Read(buffer, 0, buffersize); cont != 0; cont = file.Read(buffer, 0, buffersize))
                    {
                        byte[] sendfile = new byte[cont];
                        Array.Copy(buffer, sendfile, cont);
                        client.clientLinker.Ask((byte)RequestType.upload, new object[] { remotepath + Path.GetFileName(path), sendfile, isfirst });
                        isfirst = false;
                    }
                    file.Close();
                }
            }
        }
    }
}
