using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Serialization
{
    public interface IEventSerializer
    {
        ArraySegment<byte> SerializeObject<TEvent>(TEvent e, out int uncompressedSize);
    }
}
