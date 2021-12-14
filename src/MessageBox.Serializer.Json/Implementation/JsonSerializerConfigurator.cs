using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageBox.Serializer.Json.Implementation
{
    internal class JsonSerializerConfigurator
    {
        public JsonSerializerConfigurator(Action<JsonSerializerOptions> configuratorAction)
        {
            ConfiguratorAction = configuratorAction;
        }

        public Action<JsonSerializerOptions> ConfiguratorAction { get; }
    }
}
