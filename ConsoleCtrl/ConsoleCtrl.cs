using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleCtrl
{
    public interface ConsoleCtrl
    {
        event EventHandler OnExit;
    }
}