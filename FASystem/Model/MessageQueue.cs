using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    class MessageQueue : Queue<Message>
    {
        public List<Message> Queue { get; set; }

        public MessageQueue()
        {

        }
    }
}
