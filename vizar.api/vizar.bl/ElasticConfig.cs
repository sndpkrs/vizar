using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Nest;

namespace vizar.bl
{
    public static class ElasticConfig
    {
        public static string IndexName
        {
            get { return "stackoverflow"; }
        }

        public static string ElastisearchUrl
        {
            get { return "http://localhost:9200"; }
        }

        public static IElasticClient GetClient()
        {
            var node = new Uri("http://localhost:9200");
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex("stackoverflow").DisableDirectStreaming();
            return new ElasticClient(settings);
        }
    }
}
