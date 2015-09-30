using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Microsoft.AspNet.Identity;
using WebApplication.Models;

namespace WebApplication.Service
{
    public class DocumentService
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        [Authorize]
        public Document CreateDocument(string path, HttpPostedFileBase error)
        {
            string curId = HttpContext.Current.User.Identity.GetUserId();
            // получаем текущего пользователя
            ApplicationUser user = _db.Users.FirstOrDefault(m => m.Id == curId);

            DateTime current = DateTime.Now;
            Document doc = new Document
            {
                Size = error.ContentLength
            };

            // Получаем расширение
            string ext = error.FileName.Substring(error.FileName.LastIndexOf('.'));
            doc.Type = ext;
            doc.IsDeleted = false;
            doc.CreateDateTime = current;

            int userIdHach;
            if(user!=null)
                userIdHach = user.Id.GetHashCode();
            else
            {
                userIdHach = DateTime.Now.GetHashCode();
            }
            if (curId.IsEmpty())
                curId = "AVATAR";

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // сохраняем файл по определенному пути на сервере
                    string url = "user" + curId + "_" + current.ToString(userIdHach + "dd/MM/yyyy H:mm:ss").Replace(":", "_").Replace("/", ".").Replace(" ", "_") + ext;
                    error.SaveAs(path + url);
                    doc.Url = url;

                    _db.Documents.Add(doc);

                    _db.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }

            return doc;
        }
    }
}