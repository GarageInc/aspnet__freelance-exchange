using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using WebApplication.Models;

namespace WebApplication.Service
{
    public class DocumentService
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public Document CreateDocument(string path, HttpPostedFileBase error)
        {
            var curId = HttpContext.Current.User.Identity.GetUserId();
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
            
            // сохраняем файл по определенному пути на сервере
            string url = "user"+ curId +"_"+ current.ToString(user.Id.GetHashCode() + "dd/MM/yyyy H:mm:ss").Replace(":", "_").Replace("/", ".") + ext;
            error.SaveAs(path);
            doc.Url = url;

            _db.Documents.Add(doc);

            return doc;
        }
    }
}