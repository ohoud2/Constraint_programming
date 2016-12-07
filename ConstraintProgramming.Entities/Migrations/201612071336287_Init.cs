namespace ConstraintProgramming.Entities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Price = c.Int(nullable: false),
                        Category_Id = c.Int(nullable: false),
                        Company_Id = c.Int(nullable: false),
                        Store_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.Category_Id, cascadeDelete: true)
                .ForeignKey("dbo.Companies", t => t.Company_Id, cascadeDelete: true)
                .ForeignKey("dbo.Stores", t => t.Store_Id, cascadeDelete: true)
                .Index(t => t.Category_Id)
                .Index(t => t.Company_Id)
                .Index(t => t.Store_Id);
            
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Stores",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        LocationX = c.Double(nullable: false),
                        LocationY = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Products", "Store_Id", "dbo.Stores");
            DropForeignKey("dbo.Products", "Company_Id", "dbo.Companies");
            DropForeignKey("dbo.Products", "Category_Id", "dbo.Categories");
            DropIndex("dbo.Products", new[] { "Store_Id" });
            DropIndex("dbo.Products", new[] { "Company_Id" });
            DropIndex("dbo.Products", new[] { "Category_Id" });
            DropTable("dbo.Stores");
            DropTable("dbo.Companies");
            DropTable("dbo.Products");
            DropTable("dbo.Categories");
        }
    }
}
