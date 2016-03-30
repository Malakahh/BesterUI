using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecondTest
{
    public partial class ContactForm : Form
    {
        List<Contact> contacts = new List<Contact>();

        public ContactForm()
        {
            InitializeComponent();

            btnAddContact.Click += BtnAddContact_Click;
            btnRemoveContact.Click += BtnRemoveContact_Click;

            //Disable resizing
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            GenerateDefaultContacts();

            dataGridViewContacts.DataSource = contacts;
        }

        private void BtnRemoveContact_Click(object sender, EventArgs e)
        {
            
        }

        private void BtnAddContact_Click(object sender, EventArgs e)
        {
            if (!IsValidEmail(txtBoxEmail.Text))
            {
                txtBoxEmail.BackColor = Color.Salmon;
                return;
            }
            else
            {
                txtBoxEmail.BackColor = Color.White;
            }
        }

        private void GenerateDefaultContacts()
        {
            contacts.Add(new Contact("Dad", "", "45645778", "christian@post8.tele.dk"));
            contacts.Add(new Contact("Richard", "Johnson", "15648753", "r.j@gmail.com"));
            contacts.Add(new Contact("Gertrude", "Wright", "65498561", "gertrude32@hotmail.com"));
            contacts.Add(new Contact("Pizza", "Hut", "98139055", ""));
        }

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
