using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace vizar.models
{
    public class Post
    {
        public string Id { get; set; }

        public DateTime? CreationDate { get; set; }

        public int? Score { get; set; }

        public int? AnswerCount { get; set; }

        public string Body { get; set; }

        public string Title { get; set; }

        [Keyword(Index = true)]
        public IEnumerable<string> Tags { get; set; }

        public CompletionField Suggest { get; set; }
    }
    public class PostSuggest
    {
        public string Name { get; set; }
        public double Score { get; set; }
    }

    public class PostSuggestResponse
    {
        public IEnumerable<PostSuggest> Suggests { get; set; }
    }
}
