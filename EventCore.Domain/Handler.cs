using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore
{
    public interface IHandler<TEvent>
    {
        void Handle(TEvent e);
    }
}
