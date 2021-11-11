using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using JimmikerNetwork;
using JimmikerNetwork.Server;
using PacketType;

namespace EasyFileService
{
    public class Peer : PeerBase
    {
        const int buffersize = 1024 * 1024 * 5;

        Appllication appllication;
        string badpath = "";
        public Peer(object peer, INetServer server, Appllication _appllication) : base(peer, server)
        {
            //Console.WriteLine("aa");
            appllication = _appllication;
        }

        public override void OnOperationRequest(SendData sendData)
        {
            switch((RequestType)sendData.Code)
            {
                case RequestType.list:
                    {
                        string nowpath;
                        try
                        {
                            nowpath = appllication.rootpath + sendData.Parameters.ToString();
                        }
                        catch (Exception)
                        {
                            Reply((byte)ResponseType.listback, new string[0], 0, "");
                            break;
                        }
                        if (!Path.GetFullPath(nowpath).Contains(Path.GetFullPath(appllication.rootpath))) nowpath = appllication.rootpath;

                        List<string> alllist = new List<string>();
                        if(Directory.Exists(nowpath))
                        {
                            string[] files = Directory.GetFiles(nowpath);
                            for (int i = 0; i < files.Length; i++)
                            {
                                alllist.Add(Path.GetFileName(files[i]));
                            }
                            string[] dirs = Directory.GetDirectories(nowpath);
                            for(int i = 0; i < dirs.Length; i++)
                            {
                                alllist.Add(Path.GetFileName(dirs[i]) + "\\");
                            }
                        }
                        else
                        {
                            alllist.Add(Path.GetFileName(nowpath));
                        }
                        Reply((byte)ResponseType.listback, alllist.ToArray(), 0, "");
                        break;
                    }
                case RequestType.upload:
                    {
                        object[] getdata = (object[])sendData.Parameters;
                        string nowpath;
                        try
                        {
                            nowpath = Path.GetFullPath(appllication.rootpath + getdata[0].ToString());
                        }
                        catch (Exception)
                        {
                            break;
                        }
                        if (getdata.Length == 1)
                        {
                            if (!Path.GetFullPath(nowpath).Contains(Path.GetFullPath(appllication.rootpath))) badpath = Path.GetFullPath(appllication.rootpath + getdata[0].ToString());
                        }
                        else
                        {
                            if (badpath != "")
                            {
                                if (!nowpath.Contains(badpath))
                                {
                                    break;
                                }
                                nowpath = Path.GetFullPath(nowpath.Replace(badpath, appllication.rootpath));
                            }
                            using (FileStream file = File.Open(nowpath, (bool)getdata[2] ? FileMode.Create : FileMode.Append))
                            {
                                byte[] buffer = (byte[])getdata[1];
                                file.Write(buffer, 0, buffer.Length);
                                file.Close();
                            }
                        }

                        break;
                    }
                case RequestType.download:
                    {
                        string nowpath;
                        try
                        {
                            nowpath = Path.GetFullPath(appllication.rootpath + sendData.Parameters.ToString());
                        }
                        catch (Exception)
                        {
                            Reply((byte)ResponseType.downloadback, null, (short)DownloadReturnCode.end, "");
                            break;
                        }
                        if (!Path.GetFullPath(nowpath).Contains(Path.GetFullPath(appllication.rootpath))) nowpath = Path.GetFullPath(appllication.rootpath + Path.GetFileName(nowpath));
                        if (Path.GetFullPath(appllication.rootpath).Contains(Path.GetFullPath(nowpath)))
                        {
                            break;
                        }

                        void Sendfile(string path, string remotepath)
                        {
                            if(remotepath != "") if (remotepath[remotepath.Length - 1] != '\\') remotepath += "\\";
                            if (Directory.Exists(path))
                            {
                                Reply((byte)ResponseType.downloadback, remotepath + Path.GetFileName(path), (short)DownloadReturnCode.mkdir, "");
                                List<string> files = new List<string>(Directory.GetFiles(path));
                                files.AddRange(Directory.GetDirectories(path));
                                for (int i = 0; i < files.Count; i++)
                                {
                                    Sendfile(files[i], remotepath + Path.GetFileName(path) + "\\");
                                }
                            }
                            else if (File.Exists(path))
                            {
                                using (FileStream file = File.Open(path, FileMode.Open))
                                {
                                    byte[] buffer = new byte[buffersize];
                                    bool isfirst = true;
                                    for (int cont = file.Read(buffer, 0, buffersize); cont != 0; cont = file.Read(buffer, 0, buffersize))
                                    {
                                        byte[] sendfile = new byte[cont];
                                        Array.Copy(buffer, sendfile, cont);
                                        Reply((byte)ResponseType.downloadback, new object[] { path.Replace(nowpath, sendData.Parameters.ToString()), remotepath + Path.GetFileName(path), sendfile, isfirst }, (short)DownloadReturnCode.sendfile, "");
                                        isfirst = false;
                                    }
                                    file.Close();
                                }
                            }
                        }

                        new Thread(() =>
                        {
                            Sendfile(nowpath, "");
                            Reply((byte)ResponseType.downloadback, null, (short)DownloadReturnCode.end, "");
                        }).Start();

                        break;
                    }
                case RequestType.mkdir:
                    {
                        string nowpath;
                        try
                        {
                            nowpath = Path.GetFullPath(appllication.rootpath + sendData.Parameters.ToString());
                        }
                        catch (Exception)
                        {
                            break;
                        }

                        if (badpath != "")
                        {
                            if (!nowpath.Contains(badpath))
                            {
                                break;
                            }
                            nowpath = Path.GetFullPath(nowpath.Replace(badpath, appllication.rootpath));
                        }
                        if (!Path.GetFullPath(nowpath).Contains(Path.GetFullPath(appllication.rootpath))) nowpath = Path.GetFullPath(appllication.rootpath + Path.GetFileName(nowpath));
                        if(Path.GetFullPath(appllication.rootpath).Contains(Path.GetFullPath(nowpath)))
                        {
                            break;
                        }
                        if (!Directory.Exists(nowpath))
                        {
                            Directory.CreateDirectory(nowpath);
                        }
                        break;
                    }
                case RequestType.delete:
                    {
                        string nowpath;
                        try
                        {
                            nowpath = Path.GetFullPath(appllication.rootpath + sendData.Parameters.ToString());
                        }
                        catch (Exception)
                        {
                            break;
                        }

                        if (badpath != "")
                        {
                            if (!nowpath.Contains(badpath))
                            {
                                break;
                            }
                            nowpath = Path.GetFullPath(nowpath.Replace(badpath, appllication.rootpath));
                        }
                        if (!Path.GetFullPath(nowpath).Contains(Path.GetFullPath(appllication.rootpath)))
                        {
                            break;
                        }
                        if (Path.GetFullPath(appllication.rootpath).Contains(Path.GetFullPath(nowpath)))
                        {
                            break;
                        }
                        if (Directory.Exists(nowpath))
                        {
                            Directory.Delete(nowpath, true);
                        }
                        else if (File.Exists(nowpath))
                        {
                            File.Delete(nowpath);
                        }
                        break;
                    }
                case RequestType.move:
                    {
                        object[] getdata = (object[])sendData.Parameters;
                        string sourcepath;
                        string destinationpath;
                        try
                        {
                            sourcepath = Path.GetFullPath(appllication.rootpath + getdata[0]);
                            destinationpath = Path.GetFullPath(appllication.rootpath + getdata[1]);
                        }
                        catch (Exception)
                        {
                            break;
                        }

                        if((destinationpath + "\\").Contains(sourcepath + "\\"))
                        {
                            break;
                        }

                        if (badpath != "")
                        {
                            if (!sourcepath.Contains(badpath) || !destinationpath.Contains(badpath))
                            {
                                break;
                            }
                            sourcepath = Path.GetFullPath(sourcepath.Replace(badpath, appllication.rootpath));
                            destinationpath = Path.GetFullPath(destinationpath.Replace(badpath, appllication.rootpath));
                        }
                        if (!Path.GetFullPath(sourcepath).Contains(Path.GetFullPath(appllication.rootpath)) || !Path.GetFullPath(destinationpath).Contains(Path.GetFullPath(appllication.rootpath)))
                        {
                            break;
                        }
                        if (Path.GetFullPath(appllication.rootpath).Contains(Path.GetFullPath(sourcepath)) || Path.GetFullPath(appllication.rootpath).Contains(Path.GetFullPath(destinationpath)))
                        {
                            break;
                        }
                        if ((File.Exists(sourcepath) || Directory.Exists(sourcepath)) && !(File.Exists(destinationpath) || Directory.Exists(destinationpath)))
                        {
                            Directory.Move(sourcepath, destinationpath);
                        }
                        break;
                    }

            }
        }

        public override void OnDisconnect()
        {
            
        }
    }
}