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
using WebApplication.Service;

namespace WebApplication.Controllers
{
    [Authorize]
    public class ErrorMessageController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly DocumentService _docService = new DocumentService();

        // GET: ErrorMessage
        [Authorize(Roles = "Administrator, Moderator")]
        public async Task<ActionResult> Index()
        {
            var errorMessages = _db.ErrorMessages
                .Where(r=>r.IsDeleted==false)
                .Include(r => r.Author)
                .Include(r => r.Document)
                .OrderByDescending(r => r.CreateDateTime);

            var errorStatus = new[] {new {Id = 0, Name = "Открыто"}, new {Id = 1, Name = "Закрыто"}};
            ViewBag.ErrorStatus = new SelectList(errorStatus, "Id", "Name");

            return View(await errorMessages.ToListAsync());
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
                erM = new ErrorMessage
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
                erM.Document = _docService.CreateDocument(Server.MapPath("~/Files/ErrorMessageFiles/"), error);
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
        public ActionResult ChangeErrorMessageStatus(int? errorMessageId, string errorStatusId)
        {
            if (errorMessageId == null && errorStatusId.IsEmpty()) // == null)
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

        /// <summary>
        /// Удаление ошибки
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult Delete(int id)
        {
            var errorMessage = _db.ErrorMessages
                .Where(r => r.IsDeleted == false)
                .Where(x => x.Id == id)
                .Include(e => e.Author)
                .Include(r => r.Document)
                .First();

            if (errorMessage != null)
            {
                return PartialView("_Delete", errorMessage);
            }
            return View("Index");
        }

        /// Удаление заявки
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Moderator")]
        public void DeleteConfirmed(int id)
        {
            ErrorMessage errorMessage = _db.ErrorMessages.Find(id);

            if (errorMessage != null)
            {
                errorMessage.IsDeleted = true;

                _db.Entry(errorMessage).State = EntityState.Modified;

                _db.SaveChanges();
            }

            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}