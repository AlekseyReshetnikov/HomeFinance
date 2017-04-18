namespace HomeFinance.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Try1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Debit = c.Boolean(nullable: false),
                        ActMk = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Motions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        PersonId = c.Int(),
                        Debit = c.Boolean(nullable: false),
                        Summa = c.Int(nullable: false),
                        CategoryId = c.Int(),
                        Note = c.String(),
                        ActMk = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId)
                .ForeignKey("dbo.People", t => t.PersonId)
                .Index(t => t.PersonId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.People",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Motions", "PersonId", "dbo.People");
            DropForeignKey("dbo.Motions", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Motions", new[] { "CategoryId" });
            DropIndex("dbo.Motions", new[] { "PersonId" });
            DropTable("dbo.People");
            DropTable("dbo.Motions");
            DropTable("dbo.Categories");
        }
    }
}
