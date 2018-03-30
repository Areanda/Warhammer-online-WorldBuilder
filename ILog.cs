using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldBuilder
{
    public interface ILog
    {
        void Append(string msg);
    }
}
