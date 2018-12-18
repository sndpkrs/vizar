using Nest;

namespace vizar.models
{
    public class Product
    {
        public string Name { get; set; }
        public CompletionField Suggest {get;set;}
    }
}