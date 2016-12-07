using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstraintProgramming.Entities {
    public class SystemContext : DbContext{

        public DbSet<Store> Stores { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Category> Categories { get; set; }

        public SystemContext() : base("SystemContext") {

        }
    }
}
