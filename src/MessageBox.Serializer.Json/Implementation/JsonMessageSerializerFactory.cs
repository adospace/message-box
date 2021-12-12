using MessageBox.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageBox.Serializer.Json.Implementation
{
    internal class JsonMessageSerializerFactory : IMessageSerializerFactory
    {
        private readonly JsonSerializerOptions? _options;

        public JsonMessageSerializerFactory(JsonSerializerOptions? options = null)
        {
            _options = options;
        }

        public IMessageSerializer CreateMessageSerializer()
        {
            return new JsonMessageSerializer(_options);
        }
    }
}
