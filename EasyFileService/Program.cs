using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleCtrl;
using JimmikerNetwork;

namespace EasyFileService
{
    class Program
    {
        static bool stop = false;
        static Appllication appllication;
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("try 'EasyFileService -h' for more information");
                return;
            }

            int port = 6875;
            string getrootpath = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-p":
                        {
                            i++;
                            port = Convert.ToInt32(args[i]);
                            break;
                        }
                    case "-h":
                        {
                            Console.WriteLine(
                                "Usage: EasyFileService [options...] rootpath\n" +
                                " -h   info for command\n" +
                                " -p   server port\n"
                                );
                            return;
                        }
                    default:
                        {
                            getrootpath = args[i];
                            break;
                        }
                }
            }

            if (getrootpath == null)
            {
                Console.WriteLine("try 'EasyFileService -h' for more information");
                return;
            }

            appllication = new Appllication(getrootpath, port, System.Net.Sockets.ProtocolType.Tcp);
            appllication.GetMessage += Appllication_GetMessage;
            appllication.Start();

            ConsoleCtrl.ConsoleCtrl consoleCtrl = (Environment.OSVersion.Platform == PlatformID.Unix) || (Environment.OSVersion.Platform == PlatformID.MacOSX) || ((int)Environment.OSVersion.Platform == 128) ? (ConsoleCtrl.ConsoleCtrl)new UnixConsoleCtrl() : (ConsoleCtrl.ConsoleCtrl)new WinConsoleCtrl();

            consoleCtrl.OnExit += (sender, e) =>
            {
                Console.WriteLine("Closing...");
                appllication.Disconnect();
                SpinWait.SpinUntil(() => stop);
            };

            SpinWait.SpinUntil(() => Console.ReadLine() == "exit");
            appllication.StopUpdateThread();
        }

        private static void Appllication_GetMessage(Appllication.MessageType type, string message)
        {
            if (type == Appllication.MessageType.ServerClose)
            {
                stop = true;
                appllication.GetMessage -= Appllication_GetMessage;
                appllication = null;
            }
            Console.WriteLine(type.ToString() + ": " + message);
        }
    }
}
