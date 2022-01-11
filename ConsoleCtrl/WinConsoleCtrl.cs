using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleCtrl
{
    public class WinConsoleCtrl : ConsoleCtrl
    {
        public event EventHandler OnExit;

        [DllImport("Kernel32")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handler, bool add);

        public delegate bool ConsoleCtrlDelegate(CtrlTypes ctrlType);

        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }
        public WinConsoleCtrl()
        {
            SetConsoleCtrlHandler(t =>
            {
                OnExit?.Invoke(this, EventArgs.Empty);
                return false;
            }, true);
        }
    }
}