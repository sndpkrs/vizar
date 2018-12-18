using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using vizar.models;

namespace vizar.bl
{
    public interface ISearchService<T>
    {
        SearchResult<T> Search(string query, int page = 1, int pageSize = 10);

        Task<PostSuggestResponse> Autocomplete(string query);

        Task<IEnumerable<string>> Suggest(string query);

        SearchResult<T> FindMoreLikeThis(string query, int pageSize);

        SearchResult<Post> SearchByCategory(string query, IEnumerable<string> tags, int page, int pageSize);

        T Get(string id);
    }
}
