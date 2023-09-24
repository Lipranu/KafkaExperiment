using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Serdes
{
    public class GuidDeserializer : IDeserializer<Guid>
    {
        public Guid Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return isNull ? Guid.Empty : new Guid(data);
        }
    }
}
