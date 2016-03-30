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

        public Email(string f, string t, string b)
        {
            this.title = t;
            this.from = f;
            this.body = b;
        }

        public string Value { get { return this.from + Environment.NewLine + Environment.NewLine + this.title; } }
    }
}
