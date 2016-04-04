using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondTest
{
    public class Email
    {
        public static Email Empty = new Email(Contact.noContactYet, "", "", new List<Contact>());

        public string title;
        public Contact from;
        public string body;
        public List<Contact> receivers;

        public Email(Contact from, string title, string body, List<Contact> receivers)
        {
            this.title = title;
            this.from = from;
            this.body = body;
            this.receivers = receivers;
        }

        public string Value { get { return this.from.FirstName + Environment.NewLine + Environment.NewLine + this.title; } }
    }
}
