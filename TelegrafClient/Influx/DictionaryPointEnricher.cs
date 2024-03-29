﻿using System.Collections.Generic;
using InfluxDB.Collector.Pipeline;

namespace TelegrafClient.Influx
{
    public class DictionaryPointEnricher : IPointEnricher
    {
        readonly IReadOnlyDictionary<string, string> _tags;

        public DictionaryPointEnricher(IReadOnlyDictionary<string, string> tags)
        {
            _tags = tags;
        }

        public void Enrich(PointData point)
        {
            point.Tags = point.Tags ?? new Dictionary<string, string>();
            foreach (var tag in _tags)
            {
                if (!point.Tags.ContainsKey(tag.Key))
                    point.Tags.Add(tag.Key, tag.Value);
            }
        }
    }
}