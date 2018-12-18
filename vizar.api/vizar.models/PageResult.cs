using System;
using System.Collections.Generic;
using System.Text;

namespace vizar.models
{
    public class PageResult<T>
    {
        public int Total { get; set; }

        public int Page { get; set; }

        public IEnumerable<T> Data { get; set; }
    }
}
