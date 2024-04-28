using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common.model
{
    public class ConversationModel
    {
        public string id {  get; set; }
        public string ConKey { get; set; }
        public string Conv { get; set; }
        public string DocUrl { get; set; }

        public List<string> Pages { get; set; }
        public List<string> Summaries { get; set; }
    }
}
