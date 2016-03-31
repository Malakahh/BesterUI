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
        public event Action<Contact> ContactSelected;

        static List<Contact> contacts = new List<Contact>();
        BindingSource bs = new BindingSource();

        public ContactForm()
        {
            InitializeComponent();

            btnAddContact.Click += BtnAddContact_Click;
            btnRemoveContact.Click += BtnRemoveContact_Click;
            txtBoxPhoneNumber.KeyPress += txtBoxPhoneNumber_KeyPress;

            //Disable resizing
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            if (contacts.Count == 0)
            {
                GenerateDefaultContacts();
            }

            bs.DataSource = contacts;
            dataGridViewContacts.DataSource = bs;
        }

        private void BtnRemoveContact_Click(object sender, EventArgs e)
        {
            contacts.RemoveAt(dataGridViewContacts.SelectedRows[0].Index);
            bs.ResetBindings(false);
        }

        private void BtnAddContact_Click(object sender, EventArgs e)
        {
            if (SeededProblems.ContactForm.AddContactBtn())
            {
                return;
            }

            bool shouldReturn = false;

            if (string.IsNullOrEmpty(txtBoxEmail.Text) || !IsValidEmail(txtBoxEmail.Text))
            {
                txtBoxEmail.BackColor = Color.Salmon;
                shouldReturn = true;
            }
            else
            {
                txtBoxEmail.BackColor = Color.White;
            }

            if (string.IsNullOrEmpty(txtBoxFirstName.Text))
            {
                txtBoxFirstName.BackColor = Color.Salmon;
                shouldReturn = true;
            }
            else
            {
                txtBoxFirstName.BackColor = Color.White;
            }

            if (string.IsNullOrEmpty(txtBoxLastName.Text))
            {
                txtBoxLastName.BackColor = Color.Salmon;
                shouldReturn = true;
            }
            else
            {
                txtBoxLastName.BackColor = Color.White;
            }

            if (string.IsNullOrEmpty(txtBoxPhoneNumber.Text))
            {
                txtBoxPhoneNumber.BackColor = Color.Salmon;
                shouldReturn = true;
            }
            else
            {
                txtBoxPhoneNumber.BackColor = Color.White;
            }

            if (shouldReturn)
            {
                return;
            }

            contacts.Add(new Contact(
                txtBoxFirstName.Text,
                txtBoxLastName.Text,
                txtBoxPhoneNumber.Text,
                txtBoxEmail.Text));
            bs.ResetBindings(false);

            txtBoxEmail.Clear();
            txtBoxFirstName.Clear();
            txtBoxLastName.Clear();
            txtBoxPhoneNumber.Clear();
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

        private void txtBoxPhoneNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void dataGridViewContacts_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (ContactSelected != null)
            {
                ContactSelected(contacts[e.RowIndex]);
            }
        }
    }
}
