using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sassa.Monitor.Shared
{
    public class Asset
    {
        public int Id { get; set; }
        public string AssetType { get; set; }
        public string AssetName { get; set; }
        public string Connector {get; set;}
        public string Username { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }

    }
}
