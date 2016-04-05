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

            bs.DataSource = Contact.Contacts;
            dataGridViewContacts.DataSource = bs;
        }

        private void BtnRemoveContact_Click(object sender, EventArgs e)
        {
            Contact.Contacts.RemoveAt(dataGridViewContacts.SelectedRows[0].Index);
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

            Contact.Contacts.Add(new Contact(
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
                ContactSelected(Contact.Contacts[e.RowIndex]);
            }
        }
    }
}
