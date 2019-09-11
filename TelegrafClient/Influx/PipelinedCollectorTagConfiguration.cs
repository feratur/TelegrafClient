using System;
using System.Collections.Generic;

namespace TelegrafClient.Influx
{
    public class PipelinedCollectorTagConfiguration
    {
        readonly TelegrafCollectorConfiguration _configuration;
        readonly Dictionary<string, string> _tags = new Dictionary<string, string>();

        public PipelinedCollectorTagConfiguration(TelegrafCollectorConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _configuration = configuration;
        }

        public TelegrafCollectorConfiguration With(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                return _configuration;

            _tags[key.Trim()] = value.Trim();
            return _configuration;
        }

        public IPointEnricher CreateEnricher()
        {
            return new DictionaryPointEnricher(_tags);
        }
    }
}