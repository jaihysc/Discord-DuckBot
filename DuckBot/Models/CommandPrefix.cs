using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Models
{
    public class CommandPrefix
    {
        public Dictionary<ulong, string> GuildPrefixes { get; set; }
    }
}
