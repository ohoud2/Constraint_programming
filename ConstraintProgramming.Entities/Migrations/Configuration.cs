namespace ConstraintProgramming.Entities.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ConstraintProgramming.Entities.SystemContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ConstraintProgramming.Entities.SystemContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            if (!context.Companies.Any()) {
                context.Companies.AddOrUpdate(
                  new Company() { Id = 1, Name = "Apple"},
                  new Company() { Id = 2, Name = "Samsung" },
                  new Company() { Id = 3, Name = "Google" },
                  new Company() { Id = 4, Name = "Sony" }
                );

                context.Stores.AddOrUpdate(
                  new Store() { Id = 1, Name = "Store 1", LocationX = 10.0, LocationY = 10.0 },
                  new Store() { Id = 2, Name = "Store 2", LocationX = 20.0, LocationY = 20.0 },
                  new Store() { Id = 3, Name = "Store 3", LocationX = 30.0, LocationY = 30.0 },
                  new Store() { Id = 4, Name = "Store 4", LocationX = 40.0, LocationY = 40.0 },
                  new Store() { Id = 5, Name = "Store 5", LocationX = 50.0, LocationY = 50.0 }
                );

                context.Categories.AddOrUpdate(
                  new Category() { Id = 1, Name = "Phones" },
                  new Category() { Id = 2, Name = "Laptops" },
                  new Category() { Id = 3, Name = "Tablets" },
                  new Category() { Id = 4, Name = "PCs" },
                  new Category() { Id = 5, Name = "Watches" }
                );
                context.SaveChanges();
            }
            Random r = new Random();
            if (!context.Products.Any()) {
                var companies = context.Companies.ToList();
                var stores = context.Stores.ToList();
                var cats = context.Categories.ToList();

                for (int i = 0; i < 500; i++) {
                    context.Products.AddOrUpdate(new Product()
                    {
                        Id = i + 1,
                        Name = "Product " + i,
                        Company = companies[r.Next(0, companies.Count)],
                        Store = stores[r.Next(0, stores.Count)],
                        Category = cats[r.Next(0, cats.Count)],
                        Price = r.Next(100, 10000)
                    });
                }
                context.SaveChanges();
            }
           
        }
    }
}
