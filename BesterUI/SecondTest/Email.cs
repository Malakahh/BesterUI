using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondTest
{
    public class Email
    {
        public string title;
        public Contact from;
        public string body;
        public Contact to;

        public Email(Contact from, string title, string body, Contact to)
        {
            this.title = title;
            this.from = from;
            this.body = body;
            this.to = to;
        }

        public string Value { get { return this.from + Environment.NewLine + Environment.NewLine + this.title; } }
    }
}
