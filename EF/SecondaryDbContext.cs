using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF
{
    public class SecondaryDbContext : DbContext
    {
        public SecondaryDbContext(DbContextOptions<SecondaryDbContext> options) : base(options)
        {
        }

        public DbSet<UserProfile> UsersProfiles { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<OptionProfile> OptionsProfiles { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<LastIdTablesCompany> LastIdTablesCompanies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>()
               .HasOne(uc => uc.User)
               .WithMany(u => u.UsersProfiles)
               .HasForeignKey(uc => uc.IdUser);

            modelBuilder.Entity<UserProfile>()
              .HasOne(uc => uc.Profile)
              .WithMany(u => u.UsersProfiles)
              .HasForeignKey(uc => uc.IdProfile);

            modelBuilder.Entity<OptionProfile>()
              .HasOne(uc => uc.Profile)
              .WithMany(u => u.OptionsProfiles)
              .HasForeignKey(uc => uc.IdProfile);

            modelBuilder.Entity<OptionProfile>()
                .HasOne(uc => uc.Option)
                .WithMany(u => u.OptionsProfiles)
                .HasForeignKey(uc => uc.IdOption);

            modelBuilder.Entity<Option>()
             .HasOne(uc => uc.Module)
             .WithMany(u => u.Options)
             .HasForeignKey(uc => uc.IdModule);



            base.OnModelCreating(modelBuilder);
        }
    
    }
}
