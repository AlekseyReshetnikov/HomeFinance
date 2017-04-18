namespace HomeFinance.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Try3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notions", "Debit", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notions", "Debit");
        }
    }
}
