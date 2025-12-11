using System;
using System.Data;
using System.Windows.Forms;
using SimpleAuthApp.Data;

namespace SimpleAuthApp.Forms
{
    public partial class UserListForm : Form
    {
        private string _currentUsername;

        public UserListForm(string currentUsername)
        {
            InitializeComponent();
            _currentUsername = currentUsername;
        }

        private void UserListForm_Load(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                dgvUsers.DataSource = null;
                dgvUsers.Rows.Clear();
                dgvUsers.Columns.Clear();

                // Add columns manually
                DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
                imgCol.Name = "ProfileImage";
                imgCol.HeaderText = "Image";
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                dgvUsers.Columns.Add(imgCol);

                dgvUsers.Columns.Add("Username", "Username");
                dgvUsers.Columns.Add("FullName", "Full Name");

                DataTable dt = DatabaseHelper.GetAllUsers(_currentUsername);
                
                foreach (DataRow row in dt.Rows)
                {
                    string username = row["Username"].ToString();
                    string fullName = row["FullName"].ToString();
                    string base64 = row["ProfileImageBase64"].ToString();
                    
                    System.Drawing.Image img = null;
                    if (!string.IsNullOrEmpty(base64))
                    {
                        try
                        {
                            byte[] bytes = Convert.FromBase64String(base64);
                            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
                            {
                                img = System.Drawing.Image.FromStream(ms);
                                // Clone to avoid stream disposal issues if not careful, 
                                // but here we are creating a new Bitmap effectively.
                                // Actually, Image.FromStream needs the stream open for the lifetime of the image usually.
                                // So let's create a copy
                                img = new System.Drawing.Bitmap(img); 
                            }
                        }
                        catch { /* Ignore bad images */ }
                    }

                    dgvUsers.Rows.Add(img, username, fullName);
                }

                // Make it look better
                dgvUsers.RowTemplate.Height = 50;
                dgvUsers.Columns["Username"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvUsers.Columns["FullName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvUsers.Columns["ProfileImage"].Width = 50;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnViewProfile_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                string selectedUser = dgvUsers.SelectedRows[0].Cells["Username"].Value.ToString();
                
                ProfileForm profile = new ProfileForm(selectedUser, true); 
                profile.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a user to view.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
