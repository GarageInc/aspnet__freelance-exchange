namespace WebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ReqConfirmations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 200),
                        DocumentId = c.Int(),
                        AuthorId = c.String(maxLength: 128),
                        RequirementId = c.Int(),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .ForeignKey("dbo.Requirements", t => t.RequirementId)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorId)
                .Index(t => t.DocumentId)
                .Index(t => t.AuthorId)
                .Index(t => t.RequirementId);
            
            CreateTable(
                "dbo.Requirements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 200),
                        DocumentId = c.Int(),
                        AuthorId = c.String(maxLength: 128),
                        Status = c.Int(nullable: false),
                        Blocked = c.Boolean(nullable: false),
                        BlockedReason = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorId)
                .Index(t => t.DocumentId)
                .Index(t => t.AuthorId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Requirements", "AuthorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReqConfirmations", "AuthorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReqConfirmations", "RequirementId", "dbo.Requirements");
            DropForeignKey("dbo.Requirements", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.ReqConfirmations", "DocumentId", "dbo.Documents");
            DropIndex("dbo.Requirements", new[] { "AuthorId" });
            DropIndex("dbo.Requirements", new[] { "DocumentId" });
            DropIndex("dbo.ReqConfirmations", new[] { "RequirementId" });
            DropIndex("dbo.ReqConfirmations", new[] { "AuthorId" });
            DropIndex("dbo.ReqConfirmations", new[] { "DocumentId" });
            DropTable("dbo.Requirements");
            DropTable("dbo.ReqConfirmations");
        }
    }
}
