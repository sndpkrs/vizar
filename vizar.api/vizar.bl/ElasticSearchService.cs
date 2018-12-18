using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vizar.models;
using Nest;
using System.IO;

namespace vizar.bl
{
    public class ElasticSearchService : ISearchService<Post>
    {
        private readonly IElasticClient client;

        public ElasticSearchService()
        {
            client = ElasticConfig.GetClient();
        }

        public SearchResult<Post> Search(string query, int page, int pageSize)
        {
            var result = client.Search<Post>(x => x.Query(q => q
                                                        .MultiMatch(mp => mp
                                                            .Query(query)
                                                                .Fields(f => f
                                                                    .Fields(f1 => f1.Title, f2 => f2.Body, f3 => f3.Tags))))
                                                    .Aggregations(a => a
                                                        .Terms("by_tags", t => t
                                                            .Field(f => f.Tags)
                                                            .Size(10)))
                                                    .From(page - 1)
                                                    .Size(pageSize));

            SearchDescriptor<Post> debugQuery = new SearchDescriptor<Post>()
                    .Index("stackoverflow")
                    .Query(q => q
                        .MultiMatch(mp => mp
                            .Query(query)
                                .Fields(f => f
                                    .Fields(f1 => f1.Title, f2 => f2.Body, f3 => f3.Tags))))
                    .Aggregations(a => a
                        .Terms("by_tags", t => t
                            .Field(f => f.Tags)
                            .Size(10)))
                    .From(page - 1)
                    .Size(pageSize);

            using (MemoryStream mStream = new MemoryStream())
            {
                client.SourceSerializer.Serialize(debugQuery, mStream);
                string rawQueryText = Encoding.ASCII.GetString(mStream.ToArray());
            }


            return new SearchResult<Post>
            {
                Total = (int)result.Total,
                Page = page,
                Results = result.Documents,
                ElapsedMilliseconds = result.Took,
                AggregationsByTags = result.Aggregations.Terms("by_tags").Buckets.ToDictionary(x => x.Key, y => y.DocCount.GetValueOrDefault(0))
            };
        }

        public Post SearchById(string id)
        {
            var response = client.Search<Post>(s => s
                  .Query(q => q.Term(t => t.Field("_id").Value(id)))); //Search based on _id               

            Post post = new Post
            {
                Title = response.Documents.FirstOrDefault().Title,
                Body = response.Documents.FirstOrDefault().Body,
                CreationDate = response.Documents.FirstOrDefault().CreationDate,
                Score = response.Documents.FirstOrDefault().Score,
                AnswerCount = response.Documents.FirstOrDefault().AnswerCount,
                Tags = response.Documents.FirstOrDefault().Tags,
                Id = response.Documents.FirstOrDefault().Id,
                Suggest = response.Documents.FirstOrDefault().Suggest,

            };
            return post;
        }

        public SearchResult<Post> SearchByCategory(string query, IEnumerable<string> tags, int page = 1,
            int pageSize = 10)
        {

            var filters = tags.Select(c => new Func<QueryContainerDescriptor<Post>, QueryContainer>(x => x.Term(f => f.Tags, c))).ToArray();

            var result = client.Search<Post>(x => x
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MultiMatch(mp => mp
                                .Query(query)
                                    .Fields(f => f
                                        .Fields(f1 => f1.Title, f2 => f2.Body, f3 => f3.Tags))))
                        .Filter(f => f
                            .Bool(b1 => b1
                                .Must(filters)))))
                .Aggregations(a => a
                    .Terms("by_tags", t => t
                        .Field(f => f.Tags)
                        .Size(10)
                    )
                )
                .From(page - 1)
                .Size(pageSize));

            SearchDescriptor<Post> debugQuery = new SearchDescriptor<Post>()
                .Index("stackoverflow")
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MultiMatch(mp => mp
                                .Query(query)
                                    .Fields(f => f
                                        .Fields(f1 => f1.Title, f2 => f2.Body, f3 => f3.Tags))))
                        .Filter(f => f
                            .Bool(b1 => b1
                                .Must(filters)))))
                .Aggregations(a => a
                    .Terms("by_tags", t => t
                        .Field(f => f.Tags)
                        .Size(10)
                    )
                )
                .From(page - 1)
                .Size(pageSize);

            using (MemoryStream mStream = new MemoryStream())
            {
                client.SourceSerializer.Serialize(debugQuery, mStream);
                string rawQueryText = Encoding.ASCII.GetString(mStream.ToArray());
            }

            return new SearchResult<Post>
            {
                Total = (int)result.Total,
                Page = page,
                Results = result.Documents,
                ElapsedMilliseconds = result.Took,
                AggregationsByTags = result.Aggregations.Terms("by_tags").Buckets.ToDictionary(x => x.Key, y => y.DocCount.GetValueOrDefault(0))
            };
        }

        public async Task<PostSuggestResponse> Autocomplete(string query)
        {
            ISearchResponse<Post> searchResponse = await client.SearchAsync<Post>(s => s
                .Suggest(su => su
                    .Completion("tag-suggestions", c => c
                        .Field(f => f.Suggest)
                        .Prefix(query)
                        .SkipDuplicates()
                        .Fuzzy(f => f
                            .Fuzziness(Fuzziness.Auto)
                        )
                        .Size(5))
                ));
            var suggests = from suggest in searchResponse.Suggest["tag-suggestions"]
                from option in suggest.Options
                select new PostSuggest
                {
                    Name = option.Text,
                    Score = option.Score
                };
            return new PostSuggestResponse
            {
                Suggests = suggests
            };

        }

        public async Task<IEnumerable<string>> Suggest(string query)
        {
            ISearchResponse<Post> searchResponse = await client.SearchAsync<Post>(s => s
                .Suggest(su => su
                    .Term("post-suggestions", c => c.Text(query)
                        .Field(f => f.Body)
                        .Field(f => f.Title)
                        .Field(f => f.Tags)
                        .Size(5))
                ));
            var suggests = from suggest in searchResponse.Suggest["post-suggestions"]
                           from option in suggest.Options
                           select new PostSuggest
                           {
                               Name = option.Text,
                               Score = option.Score
                           };
            List<string> suggestions = new List<string>();
            foreach (var item in suggests)
            {
                suggestions.Add(item.Name);
            }
            return suggestions;
        }

        public SearchResult<Post> FindMoreLikeThis(string id, int pageSize)
        {
            var result = client.Search<Post>(x => x
                .Query(y => y
                    .MoreLikeThis(m => m
                        .Like(l => l.Document(d => d.Id(id)))
                        .Fields(new[] { "title", "body", "tags" })
                        .MinTermFrequency(1)
                        )).Size(pageSize));



            SearchDescriptor<Post> debugQuery = new SearchDescriptor<Post>()
                .Index("stackoverflow")
                    .Query(y => y
                    .MoreLikeThis(m => m
                        .Like(l => l.Document(d => d.Id(id)))
                        .Fields(new[] { "title", "body", "tags" })
                        .MinTermFrequency(1)
                        )).Size(pageSize);

            using (MemoryStream mStream = new MemoryStream())
            {
                client.SourceSerializer.Serialize(debugQuery, mStream);
                string rawQueryText = Encoding.ASCII.GetString(mStream.ToArray());
            }

            return new SearchResult<Post>
            {
                Total = (int)result.Total,
                Page = 1,
                Results = result.Documents
            };
        }

        public Post Get(string id)
        {
            var result = client.Get<Post>(new DocumentPath<Post>(id));
            return result.Source;
        }
    }
}
