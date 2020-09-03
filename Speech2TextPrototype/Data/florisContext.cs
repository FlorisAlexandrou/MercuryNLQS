using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Data
{
    public class florisContext : DbContext
    {
        public florisContext(DbContextOptions<florisContext> options) : base(options)
        {
        }

        public DbSet<TData> tdata { get; set; }
        public DbSet<LookupValues> lookupvalues { get; set; }
        public DbSet<DisplayTable> displayTable { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TData>().ToTable("TDATA");
            modelBuilder.Entity<LookupValues>().ToView("LOOKUP_VALUES").HasNoKey();
            modelBuilder.Entity<DisplayTable>().ToView("DISPLAY_TABLE").HasNoKey();
        }
    }
}
