using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondTest
{
    class Email
    {
        public string header;
        public string body;

        public Email(string h, string b)
        {
            this.header = h;
            this.body = b;
        }

        public string Value { get { return this.header; } }
    }
}
