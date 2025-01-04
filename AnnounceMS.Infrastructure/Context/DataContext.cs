using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AnnounceMS.Entities.Entities;

namespace AnnounceMS.Infrastructure.Context
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {

        }

       public DbSet<Announcement> Announcements { get; set; }
     

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //{
        //    if (!options.IsConfigured)
        //    {
        //        options.UseSqlServer("A FALLBACK CONNECTION STRING");
        //    }
        //}
    }
}
