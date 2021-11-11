using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacketType
{
    public enum RequestType
    {
        list,
        upload,
        download,
        mkdir,
        delete,
        move
    }
}