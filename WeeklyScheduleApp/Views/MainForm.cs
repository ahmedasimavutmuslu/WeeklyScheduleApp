using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeeklyScheduleApp.Models;

namespace WeeklyScheduleApp.Views
{
    public partial class MainForm : Form
    {
        private User _currentUser;

        public MainForm(User user)
        {
            InitializeComponent();
            _currentUser = user;

            this.Text = $"WeeklyScheduleApp - Kullanıcı: {_currentUser.Username} ({_currentUser.Role})";

            // Eğer kullanıcı Admin değilse planlama butonunu gizleyebiliriz
            if (_currentUser.Role != "Admin")
            {
                btnOpenPlanning.Enabled = false;
                btnOpenPlanning.Visible = false;
            }
        }

        // Vardiya Planlama Ekranını Açan Butonun Olayı (Click)
        private void btnOpenPlanning_Click(object sender, EventArgs e)
        {
            ShiftPlanningForm planningForm = new ShiftPlanningForm(_currentUser);
            planningForm.ShowDialog(); // Modal olarak açar
        }

        private void btnOpenUserManagement_Click(object sender, EventArgs e)
        {
            UserManagementForm userManagementForm = new UserManagementForm();
            userManagementForm.ShowDialog();
        }
    }
}
