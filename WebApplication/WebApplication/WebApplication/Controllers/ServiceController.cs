using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ServiceController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        
        /// <summary>
        /// отображение категорий
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Categories()
        {
            ViewBag.Categories = db.Categories;
            return View();
        }

        /// <summary>
        /// Добавление категорий
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Categories(Category cat)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(cat);
                db.SaveChanges();
            }
            ViewBag.Categories = db.Categories;
            return View(cat);
        }
        
        /// <summary>
        /// Удаление категории по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteCategory(int id)
        {
            Category cat = db.Categories.Find(id);
            db.Categories.Remove(cat);
            db.SaveChanges();
            return RedirectToAction("Categories");
        }


        /// <summary>
        /// отображение ролей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddRole()
        {
            ViewBag.Roles = db.Roles;
            return View();
        }

        /// <summary>
        /// Добавление ролей
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddRole(IdentityRole cat)
        {
            if (ModelState.IsValid)
            {
                db.Roles.Add(cat);
                db.SaveChanges();
            }
            ViewBag.Roles = db.Roles;
            return View(cat);
        }

        /// <summary>
        /// Удаление ролями по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteRole(int id)
        {
            IdentityRole cat = db.Roles.Find(id);
            db.Roles.Remove(cat);
            db.SaveChanges();
            return RedirectToAction("AddRole");
        }
        
        /// <summary>
        /// отображение категорий оплат
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddPropsCategory()
        {
            ViewBag.PropsCategories = db.PropsCategories;
            return View();
        }

        /// <summary>
        /// Добавление ролей
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddPropsCategory(PropsCategory cat)
        {
            if (ModelState.IsValid)
            {
                db.PropsCategories.Add(cat);
                db.SaveChanges();
            }
            ViewBag.PropsCategories = db.PropsCategories;
            return View(cat);
        }

        /// <summary>
        /// Удаление видов оплаты по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeletePropsCategory(int id)
        {
            PropsCategory cat = db.PropsCategories.Find(id);
            db.PropsCategories.Remove(cat);
            db.SaveChanges();
            return RedirectToAction("AddPropsCategory");
        }


        /// <summary>
        /// отображение категорий оплат
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddSubject()
        {
            ViewBag.Subjects = db.Subjects;
            return View();
        }

        /// <summary>
        /// Добавление ролей
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSubject(Subject cat)
        {
            if (ModelState.IsValid)
            {
                db.Subjects.Add(cat);
                db.SaveChanges();
            }
            ViewBag.Subjects = db.Subjects;
            return View(cat);
        }

        /// <summary>
        /// Удаление ролями по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteSubject(int id)
        {
            Subject cat = db.Subjects.Find(id);
            db.Subjects.Remove(cat);
            db.SaveChanges();
            return RedirectToAction("AddSubject");
        }

        public void UpdateDatabase()
        {
            var current = DateTime.Now;

            if (!db.Categories.Any())
            {
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "Сайт / программа" });
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "Домашние задачи" });
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "Контрольная" });
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "Реферат" });
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "Эссе" });
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "Перевод" });
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "Чертежи" });
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "Отчет по практике" });
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "ДИПЛОМ" });
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "КУРСОВАЯ РАБОТА" });
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "Доработка" });
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "Другое" });
                db.Categories.Add(new Category { IsDeleted = false, CreateDateTime = current, Name = "АНТИПЛАГИАТ" });

                db.SaveChanges();
            }

            if (!db.Subjects.Any())
            {
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "ПРОГРАММИРОВАНИЕ" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Математика" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Экономика" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Бухучет" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Химия" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Физика" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Чертежи" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Право" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Сопромат" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Иностранный язык" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Гидравлика" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Электротехника" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Филология / Журналистика" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "История / Политология / Культурология" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Психология / Педагогика / Логопедия" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Геология / Горно-нефтяное дело" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Автодорожное" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Философия / КСЕ" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Социология" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Биология / Экология" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Медицина" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "БЖД" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "География" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Другое" });
                db.Subjects.Add(new Subject { IsDeleted = false, CreateDateTime = current, Name = "Повышение оригинальности" });

                db.SaveChanges();
            }

            if (!db.PropsCategories.Any())
            {
                db.PropsCategories.Add(new PropsCategory { IsDeleted = false, CreateDateTime = current, Name = "Карта Сбербанка'", Info = "Желательно указать так же срок действия карты и владельца, бывает, что это спрашивается на кассе при пополнении" });
                db.PropsCategories.Add(new PropsCategory { IsDeleted = false, CreateDateTime = current, Name = "Номер телефона", Info = "Желательно указать оператор и лучше использовать при суммах менее 200р" });
                db.PropsCategories.Add(new PropsCategory { IsDeleted = false, CreateDateTime = current, Name = "QIWI Кошелек", Info = "Желательно использовать при суммах менее 200р" });

                db.SaveChanges();
            }

        }
    }
}