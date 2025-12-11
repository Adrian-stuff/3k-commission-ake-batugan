using System;
using System.Windows.Forms;
using SimpleAuthApp.Data;

namespace SimpleAuthApp.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (DatabaseHelper.ValidateUser(username, password))
                {
                    // Login success
                    ProfileForm profile = new ProfileForm(username);
                    this.Hide();
                    profile.ShowDialog();
                    this.Close(); 
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGoToSignup_Click(object sender, EventArgs e)
        {
            SignupForm signup = new SignupForm();
            this.Hide();
            signup.ShowDialog();
            this.Show(); // Show login again if they close signup or come back
        }
    }
}
