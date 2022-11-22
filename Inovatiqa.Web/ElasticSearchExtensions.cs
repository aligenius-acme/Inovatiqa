using System;
using Inovatiqa.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Inovatiqa.Core;
using Elasticsearch.Net;

namespace InovatiqaElasticSearch
{
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(
            this IServiceCollection services, IConfiguration configuration)
        {
            var url = InovatiqaDefaults.ElasticEndPoint;
            var defaultIndex = InovatiqaDefaults.DefaultIndexName;

            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex(defaultIndex)
                .BasicAuthentication(InovatiqaDefaults.ElasticUsername, InovatiqaDefaults.ElasticPassword);

            //AddDefaultMappings(settings);

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);

            //CreateIndex(client, defaultIndex);

        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings
                .DefaultMappingFor<ElasticProduct>(m => m
                    .Ignore(p => p.Sku)
                    .PropertyName(p => p.Id, "id")
                );
        }

        private static void CreateIndex(IElasticClient client, string indexName)
        {
            var createIndexResponse = client.Indices.Create(indexName, c => c
                .Settings(s => s
                    .Analysis(a => a
                        .CharFilters(cf => cf
                            .Mapping("programming_language", mcf => mcf
                                .Mappings(
                                    "c# => csharp",
                                    "C# => Csharp"
                                )
                            )
                        )
                        .Analyzers(an => an
                            .Custom("content", ca => ca
                                .CharFilters("html_strip", "programming_language")
                                .Tokenizer("standard")
                                .Filters("standard", "lowercase", "stop")
                            )
                            .Custom("categories", ca => ca
                                .CharFilters("programming_language")
                                .Tokenizer("standard")
                                .Filters("standard", "lowercase")
                            )
                        )
                    )
                )
                    .Map<ElasticProduct>(x => x
                        .AutoMap()
                        .Properties(p => p
                            .Text(t => t
                                .Name(n => n.Name)
                                .Boost(3)
                            )
                            .Text(t => t
                                .Name(n => n.Id)
                                .Analyzer("content")
                                .Boost(1)
                            )
                            .Text(t => t
                                .Name(n => n.MetaKeywords)
                                .Boost(2)
                            )
                            .Text(t => t
                                .Name(n => n.ShortDescription)
                                .Analyzer("categories")
                                .Boost(2)
                            )
                        )
                    )
            );
        }
    }
}