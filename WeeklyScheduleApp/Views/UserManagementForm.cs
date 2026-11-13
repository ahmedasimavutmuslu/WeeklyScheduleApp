using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeeklyScheduleApp.Data;
using WeeklyScheduleApp.Helpers;
using WeeklyScheduleApp.Models;

namespace WeeklyScheduleApp.Views
{
    public partial class UserManagementForm : Form
    {
        public UserManagementForm()
        {
            InitializeComponent();
        }

        private void UserManagementForm_Load(object sender, EventArgs e)
        {
            cmbRole.Items.Add("Employee");
            cmbRole.Items.Add("Admin");
            cmbRole.SelectedIndex = 0;
            LoadUsers();
        }

        private void LoadUsers()
        {
            using (var db = new AppDbContext())
            {
                dgvUsers.DataSource = db.Users.Select(u => new { u.Id, u.Username, u.Role, u.IsActive }).ToList();
            }
        }

        private void btnSaveUser_Click(object sender, EventArgs e)
        {
            string username = txtNewUsername.Text.Trim();
            string password = txtNewPassword.Text.Trim();
            string role = cmbRole.SelectedItem?.ToString() ?? "Employee";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Lütfen kullanıcı adı ve şifre giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var db = new AppDbContext())
            {
                // Aynı isimde kullanıcı var mı kontrol et
                if (db.Users.Any(u => u.Username == username))
                {
                    MessageBox.Show("Bu kullanıcı adı zaten sistemde mevcut!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = PasswordHelper.HashPassword(password),
                    Role = role,
                    IsActive = true
                };

                db.Users.Add(newUser);
                db.SaveChanges();
            }

            MessageBox.Show("Personel başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtNewUsername.Clear();
            txtNewPassword.Clear();
            LoadUsers();
        }
    }
}
