using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vizar.bl;
using vizar.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace vizar.api.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/vizar-suggests")]
    public class SuggestController : Controller
    {
        private ElasticSearchService service;
        private IHostingEnvironment _env;

        public SuggestController(IHostingEnvironment env)
        {
            _env = env;
            service = new ElasticSearchService();
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public Microsoft.AspNetCore.Mvc.OkObjectResult Get(string keyword)
        {
            return Ok(service.Autocomplete(keyword));
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("search")]
        public Microsoft.AspNetCore.Mvc.OkObjectResult Search(string keyword, int page = 1, int pageSize = 10)
        {

            var results = service.Search(keyword, page, pageSize);
            return Ok(results);
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("searchbyid")]
        public Microsoft.AspNetCore.Mvc.OkObjectResult SearchById(string id)
        {

            var result = service.SearchById(id);
            return Ok(result);
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("morelikethis")]
        public Microsoft.AspNetCore.Mvc.OkObjectResult MoreLikeThis(string id, int pageSize = 3)
        {
            return Ok(service.FindMoreLikeThis(id, pageSize));
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("searchbycategory")]
        public Microsoft.AspNetCore.Mvc.OkObjectResult SearchByCategory([FromBody]dynamic json)
        {
            string q = json.q;
            var categories = (IEnumerable<string>)json.categories.ToObject<List<string>>();
            return Ok(service.SearchByCategory(q, categories, 1, 10));
        }

        [HttpGet]
        [Route("suggest")]
        public Microsoft.AspNetCore.Mvc.OkObjectResult Suggest(string q)
        {
            var results = service.Suggest(q);
            return Ok(results);
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("index")]
        public Microsoft.AspNetCore.Mvc.OkResult Index(string fileName, int maxItems = 1000)
        {
            var indexService = new ElasticIndexService(_env);
            indexService.CreateIndex(fileName, maxItems);
            return Ok();
        }
    }
}