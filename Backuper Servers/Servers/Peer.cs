using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using UnityNetwork;
using UnityNetwork.Server;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Servers
{
    public class Peer : PeerTCPBase
    {
        Dictionary<string, string> _drive;
        Appllication appllication;
        bool on = true;
        Dictionary<string, int> _writeNow;
        Dictionary<string, Dictionary<string, Dictionary<long, Response>>> _write;
        Dictionary<string, List<string>> road;
        List<string> drive;
        public Peer(TcpClient tcpClient, NetTCPServer netTCPServer, Appllication _appllication) : base(tcpClient, netTCPServer)
        {
            Thread thread = new Thread(new ThreadStart(Writing));
            thread.Start();
            _writeNow = new Dictionary<string, int>();
            _drive = new Dictionary<string, string>();
            _write = new Dictionary<string, Dictionary<string, Dictionary<long, Response>>>();
            road = new Dictionary<string, List<string>>();
            drive = new List<string>();
            appllication = _appllication;
        }

        public override void OnDisconnect()
        {
            on = false;
        }

        public override void OnOperationRequest(Response response)
        {
            if (response.Code == 0)
            {
                a.CatchMessage("??" + response.DebugMessage);
            }
            switch (response.Code)
            {
                case 1:
                    {
                        try
                        {
                            short key = Convert.ToInt16(response.Parameters[0].ToString().Split(' ')[response.Parameters[0].ToString().Split(' ').Length - 1]);
                            string thing = "";
                            StringBuilder stringBuilder = new StringBuilder(response.Parameters[0].ToString());
                            stringBuilder.Remove(response.Parameters[0].ToString().Length - key.ToString().Length - 1, key.ToString().Length + 1);
                            thing = stringBuilder.ToString();
                            Random random = new Random(Guid.NewGuid().GetHashCode());
                            if (response.Parameters.Count != 6)
                            {
                                Reply(1, new Dictionary<byte, object>(), key, "資料不完全");
                            }
                            else
                            {
                                lock (drive)
                                {
                                    lock (road)
                                    {
                                        if (_write.ContainsKey(thing))
                                        {
                                            if (_write[thing].ContainsKey(response.Parameters[1].ToString()))
                                            {
                                                lock (_write[thing][response.Parameters[1].ToString()])
                                                {
                                                    if (!_write[thing][response.Parameters[1].ToString()].ContainsKey(Convert.ToInt64(response.Parameters[2])))
                                                    {
                                                        _write[thing][response.Parameters[1].ToString()].Add(Convert.ToInt64(response.Parameters[2]), response);
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                    _write[thing].Add(response.Parameters[1].ToString(), new Dictionary<long, Response>() { { Convert.ToInt64(response.Parameters[2]), response } });
                                                    road[thing].Add(response.Parameters[1].ToString());
                                                    _writeNow.Add(response.Parameters[1].ToString(), 0);
                                            }
                                        }
                                        else
                                        {
                                            _write.Add(thing, new Dictionary<string, Dictionary<long, Response>>());
                                            _write[thing].Add(response.Parameters[1].ToString(), new Dictionary<long, Response>());
                                            _write[thing][response.Parameters[1].ToString()].Add(Convert.ToInt64(response.Parameters[2]), response);
                                            drive.Add(thing);
                                            road.Add(thing, new List<string>() { response.Parameters[1].ToString() });
                                            _writeNow.Add(response.Parameters[1].ToString(), 0);
                                        }
                                        Reply(2, new Dictionary<byte, object>() { { 0, response.Parameters[0] }, { 1, response.Parameters[1] }, { 2, response.Parameters[2] } }, key, "資料傳送成功");
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            a.CatchMessage(e.ToString());
                        }
                        break;
                    }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        private void Writing()
        {
            while (on)
            {
                ulong FreeBytesAvailable;
                ulong TotalNumberOfBytes;
                ulong TotalNumberOfFreeBytes;
                bool success = GetDiskFreeSpaceEx(@"D:\", out FreeBytesAvailable, out TotalNumberOfBytes, out TotalNumberOfFreeBytes);

                if (!success)
                    throw new System.ComponentModel.Win32Exception();

                double free_kilobytes = (double)(Int64)TotalNumberOfFreeBytes / 1024.0;
                double free_megabytes = free_kilobytes / 1024.0;
                double free_gigabytes = free_megabytes / 1024.0;
                if (free_gigabytes > 10)
                {
                    try
                    {
                        int driveCount = 0;
                        lock (drive)
                        {
                            driveCount = drive.Count;
                        }
                        for (int i = 0; i < driveCount; i++)
                        {
                            int roadCount = 0;
                            lock (road)
                            {
                                roadCount = road[drive[i]].Count;
                            }
                            for (int ii = 0; ii < roadCount; ii++)
                            {
                                Stopwatch stopwatch = new Stopwatch();
                                stopwatch.Start();
                                bool useing = true;
                                while (useing)
                                {
                                    lock (_write[drive[i]][road[drive[i]][ii]])
                                    {
                                        if (_write[drive[i]][road[drive[i]][ii]].ContainsKey(_writeNow[road[drive[i]][ii]]))
                                        {
                                            Response response;
                                            if (_write[drive[i]][road[drive[i]][ii]].TryGetValue(_writeNow[road[drive[i]][ii]], out response))
                                            {
                                                if (response != null)
                                                {
                                                    short key = Convert.ToInt16(response.Parameters[0].ToString().Split(' ')[response.Parameters[0].ToString().Split(' ').Length - 1]);
                                                    string thing = "";
                                                    StringBuilder stringBuilder = new StringBuilder(response.Parameters[0].ToString());
                                                    stringBuilder.Remove(response.Parameters[0].ToString().Length - key.ToString().Length - 1, key.ToString().Length + 1);
                                                    thing = stringBuilder.ToString();
                                                    if (!_drive.ContainsKey(thing))
                                                    {
                                                        int iiii;
                                                        for (iiii = 1; Directory.Exists(@"D:\BackUp\CH" + iiii); iiii++)
                                                        {

                                                        }
                                                        Directory.CreateDirectory(@"D:\BackUp\CH" + iiii);
                                                        _drive.Add(thing, @"D:\BackUp\CH" + iiii);
                                                    }
                                                    string path = _drive[thing] + (new StringBuilder(response.Parameters[1].ToString())).Remove(0, 2).ToString();
                                                    byte[] bytes = (byte[])response.Parameters[3];
                                                    try
                                                    {
                                                        if (Convert.ToInt32(response.Parameters[2]) == 0)
                                                        {
                                                            if (!Directory.Exists(Path.GetDirectoryName(path)))
                                                            {
                                                                Directory.CreateDirectory(Path.GetDirectoryName(path));
                                                            }
                                                            FileStream stream = File.Create(path);
                                                            stream.Write(bytes, 0, bytes.Length);
                                                            stream.Close();
                                                        }
                                                        else
                                                        {
                                                            FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write);
                                                            stream.Write(bytes, 0, bytes.Length);
                                                            stream.Close();
                                                        }
                                                        if (Convert.ToBoolean(response.Parameters[4]))
                                                        {
                                                            useing = false;
                                                        }
                                                        _write[drive[i]][road[drive[i]][ii]].Remove(_writeNow[road[drive[i]][ii]]);
                                                        _writeNow[road[drive[i]][ii]]++;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        a.CatchMessage(e.ToString());
                                                        _write[drive[i]][road[drive[i]][ii]].Remove(_writeNow[road[drive[i]][ii]]);
                                                        Reply(1, new Dictionary<byte, object>(), Convert.ToInt16(thing[1]), "資料不完全");
                                                    }
                                                    stopwatch.Stop();
                                                    stopwatch.Reset();
                                                    stopwatch.Start();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (stopwatch.ElapsedMilliseconds > 2000)
                                            {
                                                Reply(3, new Dictionary<byte, object>() { { 0, road[drive[i]][ii] }, { 1, _writeNow[road[drive[i]][ii]] } }, 0, "資料遺失");
                                                stopwatch.Stop();
                                                stopwatch.Reset();
                                                stopwatch.Start();
                                            }
                                        }
                                    }
                                }
                                stopwatch.Stop();
                                _write[drive[i]].Remove(road[drive[i]][ii]);
                            }
                            lock (road)
                            {
                                for (int ii = 0; ii < roadCount; ii++)
                                {
                                    _writeNow.Remove(road[drive[i]][0]);
                                    road[drive[i]].RemoveAt(0);
                                }
                            }
                        }
                        lock (drive)
                        {
                            for (int ii = 0; ii < driveCount; ii++)
                            {
                                if (_write[drive[ii]].Count <= 0)
                                {
                                    _write.Remove(drive[ii]);
                                    road.Remove(drive[ii]);
                                    drive.RemoveAt(ii);
                                    ii--;
                                    driveCount--;
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        a.CatchMessage(e.ToString());
                    }
                }
            }
        }
    }
}