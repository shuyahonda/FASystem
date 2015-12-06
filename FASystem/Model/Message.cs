using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    class Message
    {
        public String Content { get; set; }
        public float Duration { get; set; }
        public Boolean isAlert { get; set; }

        public Message(String content,float duration,Boolean isAlert)
        {
            this.Content = content;
            this.Duration = duration;
            this.isAlert = isAlert;
        }

        public Message(String content) : this(content,(float)1.0,false)
        {

        }

        public Message(String content,float duration) : this(content, duration, false)
        {

        }
    }
}
