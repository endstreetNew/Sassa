using System.Collections.Generic;

namespace Sassa.BRM.ViewModels
{
    public class TDWPicklist
    {
        public TDWPicklist()
        {
            result = new List<TDWRequestMain>();
        }
        public string UnqPickList { get; set; }
        public List<TDWRequestMain> result { get; set; }
    }
}
