using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondTest
{
    class Email
    {
        public string title;
        public string from;
        public string body;
        public string to;

        public Email(string from, string title, string body, string to = "")
        {
            this.title = title;
            this.from = from;
            this.body = body;
            this.to = to;
        }

        public string Value { get { return this.from + Environment.NewLine + Environment.NewLine + this.title; } }
    }
}
