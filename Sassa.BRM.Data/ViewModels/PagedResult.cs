using System.Collections.Generic;

namespace razor.Components.Models
{
    public class PagedResult<T>
    {
        public PagedResult()
        {
            count = 0;
            result = new List<T>();
        }
        public int count;
        public List<T> result { get; set; }
    }
}
