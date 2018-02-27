using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing
{
    public struct EventPayload
    {
        private static UTF8Encoding _encoding = new UTF8Encoding(false, false);

        public EventPayload Create(object e)
        {
            // compress payload, if and only if, it yields a smaller payload

            var buffer = new MemoryStream();

            var textWriter = new StreamWriter(buffer, _encoding);
            var jsonWriter = new JsonTextWriter(textWriter);
            var sr = new JsonSerializer();
            sr.Serialize(jsonWriter, e);
            jsonWriter.Flush();

            if (buffer.Length > 128)
            {

            }
        }
    }
}
