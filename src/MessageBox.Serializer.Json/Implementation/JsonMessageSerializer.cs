using MessageBox.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageBox.Serializer.Json.Implementation
{
    internal class JsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerOptions? _serializerOptions;

        public JsonMessageSerializer(JsonSerializerOptions? serializerOptions = null)
        {
            _serializerOptions = serializerOptions;
        }

        public object Deserialize(ReadOnlySpan<byte> message, Type targetType)
        {
            return JsonSerializer.Deserialize(message, targetType, _serializerOptions) ?? throw new InvalidOperationException();
        }

        public byte[] Serialize(object model)
        {
            return JsonSerializer.SerializeToUtf8Bytes(model, _serializerOptions);
        }
    }
}
