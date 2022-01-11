using Mono.Unix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleCtrl
{
    public class UnixConsoleCtrl : ConsoleCtrl
    {
        public event EventHandler OnExit;

        UnixSignal[] signals = new UnixSignal[]{
            new UnixSignal(Mono.Unix.Native.Signum.SIGTERM),
            new UnixSignal(Mono.Unix.Native.Signum.SIGINT),
            new UnixSignal(Mono.Unix.Native.Signum.SIGUSR1)
        };
        public UnixConsoleCtrl()
        {
            Task.Factory.StartNew(() =>
            {
                // blocking call to wait for any kill signal
                int index = UnixSignal.WaitAny(signals, -1);

                if (OnExit != null)
                {
                    OnExit(this, EventArgs.Empty);
                }

            });
        }
    }
}