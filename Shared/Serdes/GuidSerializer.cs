using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Serdes
{
    public class GuidSerializer : ISerializer<Guid>
    {
        public byte[] Serialize(Guid data, SerializationContext context)
        {
            return data.ToByteArray();
        }
    }
}
