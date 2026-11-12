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

        // 📌 Program.cs'den gelen kullanıcıyı karşılayan constructor
        public MainForm(User user)
        {
            InitializeComponent();
            _currentUser = user;

            // Örnek: Form başlığında kimin giriş yaptığını gösterelim
            this.Text = $"WeeklyScheduleApp - Kullanıcı: {_currentUser.Username} ({_currentUser.Role})";
        }
    }
}
