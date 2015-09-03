namespace WebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AuthorId = c.String(maxLength: 128),
                        Karma = c.Int(nullable: false),
                        Text = c.String(),
                        ParentId = c.Int(),
                        ReqId = c.Int(),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128),
                        ApplicationUser_Id1 = c.String(maxLength: 128),
                        ApplicationUser_Id2 = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id1)
                .ForeignKey("dbo.Requests", t => t.ReqId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id2)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorId)
                .Index(t => t.AuthorId)
                .Index(t => t.ReqId)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id1)
                .Index(t => t.ApplicationUser_Id2);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false),
                        Password = c.String(nullable: false, maxLength: 100),
                        Karma = c.Int(nullable: false),
                        LastVisition = c.DateTime(nullable: false),
                        RegistrationDate = c.DateTime(nullable: false),
                        UserInfo = c.String(),
                        IsBlocked = c.Boolean(nullable: false),
                        BlockDate = c.DateTime(nullable: false),
                        BlockReason = c.String(),
                        Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                        Request_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Requests", t => t.Request_Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex")
                .Index(t => t.Request_Id);
            
            CreateTable(
                "dbo.Documents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Url = c.String(),
                        Size = c.Int(nullable: false),
                        Type = c.String(),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Contacts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContactAdress = c.String(nullable: false, maxLength: 200),
                        AuthorId = c.String(maxLength: 128),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorId)
                .Index(t => t.AuthorId);
            
            CreateTable(
                "dbo.RecallMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                        AuthorId = c.String(maxLength: 128),
                        Karma = c.Int(nullable: false),
                        ParentId = c.Int(),
                        AboutSite = c.Boolean(nullable: false),
                        UserId = c.String(maxLength: 128),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128),
                        ApplicationUser_Id1 = c.String(maxLength: 128),
                        ApplicationUser_Id2 = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id1)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id2)
                .Index(t => t.AuthorId)
                .Index(t => t.UserId)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id1)
                .Index(t => t.ApplicationUser_Id2);
            
            CreateTable(
                "dbo.ErrorMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                        AuthorId = c.String(maxLength: 128),
                        ErrorStatus = c.Int(nullable: false),
                        ForAdministration = c.Boolean(nullable: false),
                        Email = c.String(),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        Document_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.Document_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorId)
                .Index(t => t.AuthorId)
                .Index(t => t.Document_Id);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReqSolutionId = c.Int(),
                        ReqId = c.Int(),
                        Description = c.String(nullable: false),
                        DocumentId = c.Int(),
                        Checked = c.Boolean(nullable: false),
                        Closed = c.Boolean(nullable: false),
                        AddingFunds = c.Boolean(nullable: false),
                        AuthorId = c.String(maxLength: 128),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .ForeignKey("dbo.Requests", t => t.ReqId)
                .ForeignKey("dbo.RequestSolutions", t => t.ReqSolutionId)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorId)
                .Index(t => t.ReqSolutionId)
                .Index(t => t.ReqId)
                .Index(t => t.DocumentId)
                .Index(t => t.AuthorId);
            
            CreateTable(
                "dbo.Requests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 200),
                        DocumentId = c.Int(),
                        AuthorId = c.String(maxLength: 128),
                        ExecutorId = c.String(maxLength: 128),
                        Deadline = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        CategoryId = c.Int(),
                        SubjectId = c.Int(),
                        LifecycleId = c.Int(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsPaid = c.Boolean(nullable: false),
                        Checked = c.Boolean(nullable: false),
                        CanDownload = c.Boolean(nullable: false),
                        IsOnline = c.Boolean(nullable: false),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorId)
                .ForeignKey("dbo.Categories", t => t.CategoryId)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .ForeignKey("dbo.AspNetUsers", t => t.ExecutorId)
                .ForeignKey("dbo.Lifecycles", t => t.LifecycleId)
                .ForeignKey("dbo.Subjects", t => t.SubjectId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.DocumentId)
                .Index(t => t.AuthorId)
                .Index(t => t.ExecutorId)
                .Index(t => t.CategoryId)
                .Index(t => t.SubjectId)
                .Index(t => t.LifecycleId)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.Lifecycles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Opened = c.DateTime(nullable: false),
                        Distributed = c.DateTime(),
                        Proccesing = c.DateTime(),
                        Checking = c.DateTime(),
                        Closed = c.DateTime(),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Subjects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RequestSolutions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        ReqId = c.Int(),
                        DocumentId = c.Int(),
                        AuthorId = c.String(maxLength: 128),
                        Comment = c.String(),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .ForeignKey("dbo.Requests", t => t.ReqId)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorId)
                .Index(t => t.ReqId)
                .Index(t => t.DocumentId)
                .Index(t => t.AuthorId);
            
            CreateTable(
                "dbo.Props",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PropsCategoryId = c.Int(),
                        Number = c.String(nullable: false, maxLength: 200),
                        AuthorId = c.String(maxLength: 128),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PropsCategories", t => t.PropsCategoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorId)
                .Index(t => t.PropsCategoryId)
                .Index(t => t.AuthorId);
            
            CreateTable(
                "dbo.PropsCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Info = c.String(nullable: false, maxLength: 200),
                        CreateDateTime = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Comments", "AuthorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.RecallMessages", "ApplicationUser_Id2", "dbo.AspNetUsers");
            DropForeignKey("dbo.Comments", "ApplicationUser_Id2", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Requirements", "AuthorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.RequestSolutions", "AuthorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Requests", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReqConfirmations", "AuthorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReqConfirmations", "RequirementId", "dbo.Requirements");
            DropForeignKey("dbo.Requirements", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.ReqConfirmations", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.RecallMessages", "ApplicationUser_Id1", "dbo.AspNetUsers");
            DropForeignKey("dbo.Props", "AuthorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Props", "PropsCategoryId", "dbo.PropsCategories");
            DropForeignKey("dbo.Payments", "AuthorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Payments", "ReqSolutionId", "dbo.RequestSolutions");
            DropForeignKey("dbo.RequestSolutions", "ReqId", "dbo.Requests");
            DropForeignKey("dbo.RequestSolutions", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.Payments", "ReqId", "dbo.Requests");
            DropForeignKey("dbo.Requests", "SubjectId", "dbo.Subjects");
            DropForeignKey("dbo.AspNetUsers", "Request_Id", "dbo.Requests");
            DropForeignKey("dbo.Requests", "LifecycleId", "dbo.Lifecycles");
            DropForeignKey("dbo.Requests", "ExecutorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Requests", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.Comments", "ReqId", "dbo.Requests");
            DropForeignKey("dbo.Requests", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Requests", "AuthorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Payments", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ErrorMessages", "AuthorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ErrorMessages", "Document_Id", "dbo.Documents");
            DropForeignKey("dbo.RecallMessages", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.RecallMessages", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.RecallMessages", "AuthorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Comments", "ApplicationUser_Id1", "dbo.AspNetUsers");
            DropForeignKey("dbo.Contacts", "AuthorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Comments", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Documents", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.Requirements", new[] { "AuthorId" });
            DropIndex("dbo.Requirements", new[] { "DocumentId" });
            DropIndex("dbo.ReqConfirmations", new[] { "RequirementId" });
            DropIndex("dbo.ReqConfirmations", new[] { "AuthorId" });
            DropIndex("dbo.ReqConfirmations", new[] { "DocumentId" });
            DropIndex("dbo.Props", new[] { "AuthorId" });
            DropIndex("dbo.Props", new[] { "PropsCategoryId" });
            DropIndex("dbo.RequestSolutions", new[] { "AuthorId" });
            DropIndex("dbo.RequestSolutions", new[] { "DocumentId" });
            DropIndex("dbo.RequestSolutions", new[] { "ReqId" });
            DropIndex("dbo.Requests", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.Requests", new[] { "LifecycleId" });
            DropIndex("dbo.Requests", new[] { "SubjectId" });
            DropIndex("dbo.Requests", new[] { "CategoryId" });
            DropIndex("dbo.Requests", new[] { "ExecutorId" });
            DropIndex("dbo.Requests", new[] { "AuthorId" });
            DropIndex("dbo.Requests", new[] { "DocumentId" });
            DropIndex("dbo.Payments", new[] { "AuthorId" });
            DropIndex("dbo.Payments", new[] { "DocumentId" });
            DropIndex("dbo.Payments", new[] { "ReqId" });
            DropIndex("dbo.Payments", new[] { "ReqSolutionId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.ErrorMessages", new[] { "Document_Id" });
            DropIndex("dbo.ErrorMessages", new[] { "AuthorId" });
            DropIndex("dbo.RecallMessages", new[] { "ApplicationUser_Id2" });
            DropIndex("dbo.RecallMessages", new[] { "ApplicationUser_Id1" });
            DropIndex("dbo.RecallMessages", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.RecallMessages", new[] { "UserId" });
            DropIndex("dbo.RecallMessages", new[] { "AuthorId" });
            DropIndex("dbo.Contacts", new[] { "AuthorId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.Documents", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.AspNetUsers", new[] { "Request_Id" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Comments", new[] { "ApplicationUser_Id2" });
            DropIndex("dbo.Comments", new[] { "ApplicationUser_Id1" });
            DropIndex("dbo.Comments", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.Comments", new[] { "ReqId" });
            DropIndex("dbo.Comments", new[] { "AuthorId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.Requirements");
            DropTable("dbo.ReqConfirmations");
            DropTable("dbo.PropsCategories");
            DropTable("dbo.Props");
            DropTable("dbo.RequestSolutions");
            DropTable("dbo.Subjects");
            DropTable("dbo.Lifecycles");
            DropTable("dbo.Requests");
            DropTable("dbo.Payments");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.ErrorMessages");
            DropTable("dbo.RecallMessages");
            DropTable("dbo.Contacts");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.Documents");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Comments");
            DropTable("dbo.Categories");
        }
    }
}
