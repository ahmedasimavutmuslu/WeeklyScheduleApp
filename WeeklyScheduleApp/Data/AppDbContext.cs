using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WeeklyScheduleApp.Helpers;
using WeeklyScheduleApp.Models;
namespace WeeklyScheduleApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Shift> Shifts => Set<Shift>();
        public DbSet<ShiftSchedule> ShiftSchedules => Set<ShiftSchedule>();
        public DbSet<TaskAllocation> TaskAllocations => Set<TaskAllocation>();
        public DbSet<UserTaskHistory> UserTaskHistories => Set<UserTaskHistory>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // veritabani dosyasini olustur
            optionsBuilder.UseSqlite("Data Source=weeklyscheduleapp.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // varsayilan admin ve calisan ekleme
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = PasswordHelper.HashPassword("1234"), 
                    Role = "Admin",
                    IsActive = true
                },
                new User
                {
                    Id = 2,
                    Username = "ahmet",
                    PasswordHash = PasswordHelper.HashPassword("1234"), 
                    Role = "Employee",
                    IsActive = true
                }
            );
        }
    }
}
