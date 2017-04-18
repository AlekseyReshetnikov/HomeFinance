namespace HomeFinance.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Try2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Assets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        BeginDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        ActMk = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Notions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        NeedAsset = c.Boolean(nullable: false),
                        ActMk = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Categories", "NotionId", c => c.Int());
            AddColumn("dbo.Motions", "NotionId", c => c.Int());
            AddColumn("dbo.Motions", "AssetId", c => c.Int());
            AddColumn("dbo.People", "ActMk", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Categories", "NotionId");
            CreateIndex("dbo.Motions", "NotionId");
            CreateIndex("dbo.Motions", "AssetId");
            AddForeignKey("dbo.Categories", "NotionId", "dbo.Notions", "Id");
            AddForeignKey("dbo.Motions", "AssetId", "dbo.Assets", "Id");
            AddForeignKey("dbo.Motions", "NotionId", "dbo.Notions", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Motions", "NotionId", "dbo.Notions");
            DropForeignKey("dbo.Motions", "AssetId", "dbo.Assets");
            DropForeignKey("dbo.Categories", "NotionId", "dbo.Notions");
            DropIndex("dbo.Motions", new[] { "AssetId" });
            DropIndex("dbo.Motions", new[] { "NotionId" });
            DropIndex("dbo.Categories", new[] { "NotionId" });
            DropColumn("dbo.People", "ActMk");
            DropColumn("dbo.Motions", "AssetId");
            DropColumn("dbo.Motions", "NotionId");
            DropColumn("dbo.Categories", "NotionId");
            DropTable("dbo.Notions");
            DropTable("dbo.Assets");
        }
    }
}
