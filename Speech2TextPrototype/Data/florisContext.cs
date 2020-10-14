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
        public DbSet<LookupTable> lookupvalues { get; set; }
        public DbSet<DisplayTable> displayTable { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TData>().ToTable("TDATA");
            modelBuilder.Entity<LookupTable>().ToView("LOOKUP_VALUES").HasNoKey();
            modelBuilder.Entity<DisplayTable>().ToTable("DISPLAY_TABLE").HasKey(dt => new { dt.UUID, dt.ROWNUMBER}); ;
        }
    }
}
