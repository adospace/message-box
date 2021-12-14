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
        private readonly IEnumerable<JsonSerializerConfigurator> _configurators;

        public JsonMessageSerializerFactory(IEnumerable<JsonSerializerConfigurator> configurators)
        {
            _configurators = configurators;
        }

        public IMessageSerializer CreateMessageSerializer()
        {
            JsonSerializerOptions? options = null;
            if (_configurators.Any())
            {
                options = new();
                foreach (var configurator in _configurators)
                {
                    configurator.ConfiguratorAction(options);
                }
            }

            return new JsonMessageSerializer(options);
        }
    }
}
