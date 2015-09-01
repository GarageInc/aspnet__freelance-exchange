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
    }
}