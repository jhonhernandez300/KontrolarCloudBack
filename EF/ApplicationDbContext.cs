using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EF
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<LastId> LastIds { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserCompany> UsersCompanies { get; set; }
        public DbSet<UserProfile> UsersProfiles { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<OptionProfile> OptionsProfiles { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Module> Modules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Option>()
               .HasOne(uc => uc.Module)
               .WithMany(u => u.Options)
               .HasForeignKey(uc => uc.IdModule);

            modelBuilder.Entity<OptionProfile>()
                .HasOne(uc => uc.Profile)
                .WithMany(u => u.OptionsProfiles)
                .HasForeignKey(uc => uc.IdProfile);

            modelBuilder.Entity<OptionProfile>()
                .HasOne(uc => uc.Option)
                .WithMany(u => u.OptionsProfiles)
                .HasForeignKey(uc => uc.IdOption);

            modelBuilder.Entity<UserCompany>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserCompanies)
                .HasForeignKey(uc => uc.IdUser);

            modelBuilder.Entity<UserCompany>()
                .HasOne(uc => uc.Company)
                .WithMany(c => c.UserCompanies)
                .HasForeignKey(uc => uc.IdCompany);

            modelBuilder.Entity<UserProfile>()
               .HasOne(uc => uc.User)
               .WithMany(u => u.UsersProfiles)
               .HasForeignKey(uc => uc.IdUser);

            modelBuilder.Entity<UserProfile>()
              .HasOne(uc => uc.Profile)
              .WithMany(u => u.UsersProfiles)
              .HasForeignKey(uc => uc.IdProfile);

            base.OnModelCreating(modelBuilder);
        }
    }
}
