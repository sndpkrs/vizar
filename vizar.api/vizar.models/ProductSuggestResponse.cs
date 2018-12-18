using System.Collections.Generic;

namespace vizar.models
{
    public class ProductSuggestResponse
    {
        public IEnumerable<ProductSuggest> Suggests { get; set; }
    }

    public class ProductSuggest
    {
        public string Name { get; set; }
        public double Score { get; set; }  
    }
}