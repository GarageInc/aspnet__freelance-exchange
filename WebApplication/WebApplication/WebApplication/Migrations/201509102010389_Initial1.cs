namespace WebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.Comments", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.Documents", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.Contacts", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.RecallMessages", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.ErrorMessages", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.Payments", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.Requests", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.Lifecycles", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.RequestSolutions", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.Subjects", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.Props", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.PropsCategories", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.RequirementConfirmations", "DateOfDeleting", c => c.DateTime());
            AddColumn("dbo.Requirements", "DateOfDeleting", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Requirements", "DateOfDeleting");
            DropColumn("dbo.RequirementConfirmations", "DateOfDeleting");
            DropColumn("dbo.PropsCategories", "DateOfDeleting");
            DropColumn("dbo.Props", "DateOfDeleting");
            DropColumn("dbo.Subjects", "DateOfDeleting");
            DropColumn("dbo.RequestSolutions", "DateOfDeleting");
            DropColumn("dbo.Lifecycles", "DateOfDeleting");
            DropColumn("dbo.Requests", "DateOfDeleting");
            DropColumn("dbo.Payments", "DateOfDeleting");
            DropColumn("dbo.ErrorMessages", "DateOfDeleting");
            DropColumn("dbo.RecallMessages", "DateOfDeleting");
            DropColumn("dbo.Contacts", "DateOfDeleting");
            DropColumn("dbo.Documents", "DateOfDeleting");
            DropColumn("dbo.Comments", "DateOfDeleting");
            DropColumn("dbo.Categories", "DateOfDeleting");
        }
    }
}
