using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondTest
{
    public class Contact
    {
        public static List<Contact> Contacts = new List<Contact>();
        public static Contact User;

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public Contact(string FirstName, string LastName, string PhoneNumber, string Email)
        {
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.PhoneNumber = PhoneNumber;
            this.Email = Email;
        }

        public static void GenerateDefaultContacts()
        {
            User = new Contact("Me", "", "", "");

            Contacts.Add(new Contact("Dad", "", "45645778", "christian@post8.tele.dk"));
            Contacts.Add(new Contact("Richard", "Johnson", "15648753", "r.j@gmail.com"));
            Contacts.Add(new Contact("Gertrude", "Wright", "65498561", "gertrude32@hotmail.com"));
            Contacts.Add(new Contact("Pizza", "Hut", "98139055", ""));
        }
    }
}
