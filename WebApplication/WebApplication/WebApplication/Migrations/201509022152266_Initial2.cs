namespace WebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Comments", "AddDateTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Comments", "AddDateTime", c => c.DateTime(nullable: false));
        }
    }
}
