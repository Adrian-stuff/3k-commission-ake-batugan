using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SimpleAuthApp.Data;

namespace SimpleAuthApp.Forms
{
    public partial class ProfileForm : Form
    {
        private string _username;
        private bool _isReadOnly;

        public ProfileForm(string username, bool isReadOnly = false)
        {
            InitializeComponent();
            _username = username;
            _isReadOnly = isReadOnly;
            
            if (_isReadOnly)
            {
                lblWelcome.Text = $"{_username}'s Profile";
                btnUpload.Visible = false;
                btnLogout.Visible = false; // Or change text to "Close"
                btnBrowseUsers.Visible = false;
                
                // Hide posting controls
                lblNewPost.Visible = false;
                txtPostContent.Visible = false;
                btnPost.Visible = false;

                this.Text = $"{_username} - SimpleAuthApp";
            }
            else
            {
                lblWelcome.Text = $"Welcome, {_username}!";
            }
        }

        private void ProfileForm_Load(object sender, EventArgs e)
        {
            LoadProfileData();
            LoadPosts();
        }

        private void LoadProfileData()
        {
            try
            {
                // Image
                string base64Image = DatabaseHelper.GetUserImage(_username);
                if (!string.IsNullOrEmpty(base64Image))
                {
                    byte[] imageBytes = Convert.FromBase64String(base64Image);
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        picProfile.Image = Image.FromStream(ms);
                    }
                }
                
                // Name
                string fullName = DatabaseHelper.GetUserFullName(_username);
                lblFullName.Text = string.IsNullOrEmpty(fullName) ? "@" + _username : fullName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadPosts()
        {
            try
            {
                var dt = DatabaseHelper.GetUserPosts(_username);
                dgvPosts.DataSource = dt;
                
                if(dgvPosts.Columns["Content"] != null) dgvPosts.Columns["Content"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                if(dgvPosts.Columns["CreatedAt"] != null) dgvPosts.Columns["CreatedAt"].Width = 120;
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to load posts: {ex.Message}");
            }
        }

        private void btnPost_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtPostContent.Text)) return;
            try
            {
                DatabaseHelper.CreatePost(_username, txtPostContent.Text);
                txtPostContent.Clear();
                LoadPosts();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error posting: {ex.Message}");
            }
        }

        private void LoadProfileImage()
        {
            // Replaced by LoadProfileData, keeping empty or removing references
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                        string base64String = Convert.ToBase64String(imageBytes);

                        DatabaseHelper.UpdateUserImage(_username, base64String);
                        
                        // Update UI
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            picProfile.Image = Image.FromStream(ms);
                        }
                        
                        MessageBox.Show("Profile picture updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error uploading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBrowseUsers_Click(object sender, EventArgs e)
        {
            UserListForm userList = new UserListForm(_username);
            this.Hide();
            userList.ShowDialog();
            this.Show();
        }
    }
}
