
using Microsoft.AspNet.Identity;
using WebApplication.Service;

namespace WebApplication.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;
    using WebApplication.Models;

    public class RequirementConfirmationController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly DocumentService _docService = new DocumentService();

        // GET: RequirementConfirmations
        public async Task<ActionResult> Index()
        {
            var requirementConfirmations = _db.RequirementConfirmations
                .Where(r=>r.IsDeleted==false)
                .Include(r => r.Author)
                .Include(r => r.Document);
            return View(await requirementConfirmations.ToListAsync());
        }

        // GET: RequirementConfirmations/Create
        public ActionResult Create(int id)
        {
            ViewBag.RequirementId = id;
            return PartialView("_Create");//View();
        }

        // POST: RequirementConfirmations/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void Create(int requirementId, [Bind(Include = "Id,Description")] RequirementConfirmation requirementConfirmation, HttpPostedFileBase error)
        {
            if (ModelState.IsValid)
            {
                // Получим заявку
                var requirement = _db.Requirements.Find(requirementId);

                // если получен файл
                if (error != null)
                {
                    requirementConfirmation.Document = _docService.CreateDocument(Server.MapPath("~/Files/RequirementConfirmationFiles/"), error);
                }
                else
                    requirementConfirmation.Document = null;

                var curId = this.User.Identity.GetUserId();
                var user = _db.Users.Find(curId);
                
                requirementConfirmation.Requirement = requirement;
                requirementConfirmation.RequirementId = requirementId;

                requirementConfirmation.CreateDateTime = DateTime.Now;
                requirementConfirmation.IsDeleted = false;
                requirementConfirmation.Author = user;
                requirementConfirmation.AuthorId = user.Id;
                


                _db.RequirementConfirmations.Add(requirementConfirmation);
                _db.SaveChangesAsync();
            }

            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        // GET: RequirementConfirmations/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequirementConfirmation requirementConfirmation = await _db.RequirementConfirmations.FindAsync(id);
            if (requirementConfirmation == null)
            {
                return HttpNotFound();
            }

            return View(requirementConfirmation);
        }

        // POST: RequirementConfirmations/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Description")] RequirementConfirmation requirementConfirmation, HttpPostedFileBase error)
        {
            if (ModelState.IsValid)
            {
                var curId = this.User.Identity.GetUserId();

                if (requirementConfirmation.AuthorId != curId)
                    return Content("Извините, но Вы не автор данной записи и не можете её редактировать");

                var requirConfBase = _db.RequirementConfirmations.Find(requirementConfirmation.Id);

                if (error != null)
                {
                    requirConfBase.Document = _docService.CreateDocument(Server.MapPath("~/Files/RequirementConfirmationFiles/"), error);
                }

                requirConfBase.Description = requirementConfirmation.Description;
                
                _db.Entry(requirConfBase).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(requirementConfirmation);
        }

        // GET: RequirementConfirmations/Delete/5
        [HttpGet]
        public ActionResult Delete(int id)
        {
            RequirementConfirmation requirementConfirmation = _db.RequirementConfirmations
                .Where(x => x.Id == id)
                .Where(x => x.IsDeleted == false)
                .Include(x => x.Author)
                .First();

            if (requirementConfirmation != null)
            {
                //получаем категорию
                return PartialView("_Delete", requirementConfirmation);
            }
            return View("Index");
        }

        // Удаление заявки
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public void DeleteConfirmed(int id)
        {
            RequestSolution requestSol = _db.RequestSolutions.Find(id);
            var curId = HttpContext.User.Identity.GetUserId();

            // получаем текущего пользователя
            ApplicationUser user = _db.Users.First(m => m.Id == curId);
            if (requestSol != null && requestSol.Author.Id == user.Id)
            {
                requestSol.IsDeleted = true;

                _db.Entry(requestSol).State = EntityState.Modified;

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
