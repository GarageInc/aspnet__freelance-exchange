using WebApplication.Models;

namespace WebApplication.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<WebApplication.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(WebApplication.Models.ApplicationDbContext context)
        {
            var rm = new RoleManager<IdentityRole>
                (new RoleStore<IdentityRole>(context));
            rm.Create(new IdentityRole("Administrator"));
            rm.Create(new IdentityRole("Moderator"));
            rm.Create(new IdentityRole("User"));

            ApplicationDbContext db = new ApplicationDbContext();
            
            db.Categories.Add(new Category{Name = "���� / ���������"});
            db.Categories.Add(new Category{ Name = "�������� ������" });
            db.Categories.Add(new Category{ Name = "�����������" });
            db.Categories.Add(new Category{ Name = "�������" });
            db.Categories.Add(new Category{ Name = "����" });
            db.Categories.Add(new Category{ Name = "�������" });
            db.Categories.Add(new Category{ Name = "�������" });
            db.Categories.Add(new Category{ Name = "����� �� ��������" });
            db.Categories.Add(new Category{ Name = "������" });
            db.Categories.Add(new Category{ Name = "�������� ������" });
            db.Categories.Add(new Category{ Name = "���������" });
            db.Categories.Add(new Category{ Name = "������" });
            db.Categories.Add(new Category{ Name = "�����������" });
            
            db.Subjects.Add(new Subject { Name = "����������������" });
            db.Subjects.Add(new Subject { Name = "����������" });
            db.Subjects.Add(new Subject { Name = "���������" });
            db.Subjects.Add(new Subject { Name = "�������" });
            db.Subjects.Add(new Subject { Name = "�����" });
            db.Subjects.Add(new Subject { Name = "������" });
            db.Subjects.Add(new Subject { Name = "�������" });
            db.Subjects.Add(new Subject { Name = "�����" });
            db.Subjects.Add(new Subject { Name = "��������" });
            db.Subjects.Add(new Subject { Name = "����������� ����" });
            db.Subjects.Add(new Subject { Name = "����������" });
            db.Subjects.Add(new Subject { Name = "��������������" });
            db.Subjects.Add(new Subject { Name = "��������� / ������������" });
            db.Subjects.Add(new Subject { Name = "������� / ����������� / �������������" });
            db.Subjects.Add(new Subject { Name = "���������� / ���������� / ���������" });
            db.Subjects.Add(new Subject { Name = "�������� / �����-�������� ����" });
            db.Subjects.Add(new Subject { Name = "������������" });
            db.Subjects.Add(new Subject { Name = "��������� / ���" });
            db.Subjects.Add(new Subject { Name = "����������" });
            db.Subjects.Add(new Subject { Name = "�������� / ��������" });
            db.Subjects.Add(new Subject { Name = "��������" });
            db.Subjects.Add(new Subject { Name = "���" });
            db.Subjects.Add(new Subject { Name = "���������" });
            db.Subjects.Add(new Subject { Name = "������" });
            db.Subjects.Add(new Subject { Name = "��������� ��������������" });
            
            db.PropsCategories.Add(new PropsCategory { Name = "����� ���������'", Info = "���������� ������� ��� �� ���� �������� ����� � ���������, ������, ��� ��� ������������ �� ����� ��� ����������" });
            db.PropsCategories.Add(new PropsCategory { Name = "����� ��������", Info = "���������� ������� �������� � ����� ������������ ��� ������ ����� 200�" });
            db.PropsCategories.Add(new PropsCategory { Name = "QIWI �������", Info = "���������� ������������ ��� ������ ����� 200�" });

            db.SaveChanges();

        }
    }
}
