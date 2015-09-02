using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication.Models;
using Microsoft.AspNet.Identity;
using System.Web.WebPages;

namespace WebApplication.Controllers
{
    [Authorize]
    public class ErrorMessageController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        // GET: ErrorMessage
        [Authorize(Roles = "Administrator, Moderator")]
        public async Task<ActionResult> Index()
        {

            var errorMessages = _db.ErrorMessages
                .Include(e => e.Author)
                .Include(r=>r.Document)
                .OrderByDescending(r => r.CreateDateTime);
            var errorStatus = new[] { new { Id = 0, Name = "Открыто" }, new { Id = 1, Name = "Закрыто" } };
            ViewBag.ErrorStatus = new SelectList(errorStatus, "Id", "Name");

            return View(await errorMessages.ToListAsync());
        }
        
        // POST: ErrorMessage/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            ErrorMessage errorMessage = await _db.ErrorMessages.FindAsync(id);
            _db.ErrorMessages.Remove(errorMessage);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var curId = HttpContext.User.Identity.GetUserId();
            // получаем текущего пользователя
            ApplicationUser user = _db.Users.FirstOrDefault(m => m.Id == curId);
            if (user != null)
            {
                return View();
            }
            return RedirectToAction("LogOff", "Account");
        }

        // Создание новой заявки об ошибке
        [HttpPost]
        public void Create(string forAdm, HttpPostedFileBase error)
        {
            var uploadText = Request.Params["Text"];
            var curId = HttpContext.User.Identity.GetUserId();
            // получаем текущего пользователя
            ApplicationUser user = _db.Users.FirstOrDefault(m => m.Id == curId);
            ErrorMessage erM;
            if (user != null)
            {
                erM= new ErrorMessage
                {
                    Author = user,
                    AuthorId = user.Id,
                    CreateDateTime = DateTime.Now,
                    ErrorStatus = 0
                };
            }
            else
            {
                erM = new ErrorMessage
                {
                    CreateDateTime = DateTime.Now,
                    ErrorStatus = 0
                };
            }

            // если получен файл
            if (error != null)
            {
                DateTime current = DateTime.Now;

                Document doc = new Document();
                doc.Size = error.ContentLength;
                // Получаем расширение
                string ext = error.FileName.Substring(error.FileName.LastIndexOf('.'));
                doc.Type = ext;
                // сохраняем файл по определенному пути на сервере
                string path = current.ToString(user.Id.GetHashCode()+"dd/MM/yyyy H:mm:ss").Replace(":", "_").Replace("/", ".") + ext;
                error.SaveAs(Server.MapPath("~/Files/ErrorMessageFiles/" + path));
                doc.Url = path;

                erM.Document = doc;
                _db.Documents.Add(doc);
            }
            else
                erM.Document = null;

            if (uploadText == null)
                erM.Text = "";
            else erM.Text = uploadText;

            if (forAdm == "1")
            {
                erM.ForAdministration = true;
                var email = Request.Params["email"].ToString();
                erM.Email = email;
            }
            else
                erM.ForAdministration = false;
            
            // Добавляем заявку с возможно приложенными документами
            _db.ErrorMessages.Add(erM);
            user.ErrorMessages.Add(erM);
            _db.Entry(user).State = EntityState.Modified;
            
            _db.SaveChanges();
            Response.Redirect(Request.UrlReferrer.AbsoluteUri);                
        }


        /// <summary>
        /// Редактирование статусов ошибок
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator, Moderator")]
        [Authorize]
        public ActionResult ChangeErrorMessageStatus(int? errorMessageId, string errorStatusId)
        {
            if (errorMessageId == null && errorStatusId.IsEmpty())// == null)
            {
                return RedirectToAction("Index");
            }
            ErrorMessage req = _db.ErrorMessages.Find(errorMessageId);
            ApplicationUser ex = _db.Users.Find(errorStatusId);

            var erSt = int.Parse(errorStatusId);

            if (req == null && ex == null)
            {
                return RedirectToAction("Index");
            }
            req.ErrorStatus = erSt;
            _db.Entry(req).State = EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            ErrorMessage erMes = _db.ErrorMessages.Find(id);

            if (erMes != null)
            {
                return PartialView("_Details", erMes);
            }
            return View("Index");
        }

    }
}
