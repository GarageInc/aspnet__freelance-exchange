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

namespace WebApplication.Controllers
{
    public class ReqConfirmationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ReqConfirmations
        public async Task<ActionResult> Index()
        {
            var ReqConfirmations = db.ReqConfirmations.Include(r => r.Author).Include(r => r.Document).Include(r => r.Requirement);
            return View(await ReqConfirmations.ToListAsync());
        }

        // GET: ReqConfirmations/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReqConfirmation requirementConfirmation = await db.ReqConfirmations.FindAsync(id);
            if (requirementConfirmation == null)
            {
                return HttpNotFound();
            }
            return View(requirementConfirmation);
        }

        // GET: ReqConfirmations/Create
        public ActionResult Create()
        {
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "Name");
            ViewBag.DocumentId = new SelectList(db.Documents, "Id", "Url");
            ViewBag.RequirementId = new SelectList(db.Requirements, "Id", "Name");
            return View();
        }

        // POST: ReqConfirmations/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Description,DocumentId,AuthorId,RequirementId")] ReqConfirmation requirementConfirmation)
        {
            if (ModelState.IsValid)
            {
                db.ReqConfirmations.Add(requirementConfirmation);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.AuthorId = new SelectList(db.Users, "Id", "Name", requirementConfirmation.AuthorId);
            ViewBag.DocumentId = new SelectList(db.Documents, "Id", "Url", requirementConfirmation.DocumentId);
            ViewBag.RequirementId = new SelectList(db.Requirements, "Id", "Name", requirementConfirmation.RequirementId);
            return View(requirementConfirmation);
        }

        // GET: ReqConfirmations/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReqConfirmation requirementConfirmation = await db.ReqConfirmations.FindAsync(id);
            if (requirementConfirmation == null)
            {
                return HttpNotFound();
            }
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "Name", requirementConfirmation.AuthorId);
            ViewBag.DocumentId = new SelectList(db.Documents, "Id", "Url", requirementConfirmation.DocumentId);
            ViewBag.RequirementId = new SelectList(db.Requirements, "Id", "Name", requirementConfirmation.RequirementId);
            return View(requirementConfirmation);
        }

        // POST: ReqConfirmations/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,DocumentId,AuthorId,RequirementId")] ReqConfirmation requirementConfirmation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(requirementConfirmation).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "Name", requirementConfirmation.AuthorId);
            ViewBag.DocumentId = new SelectList(db.Documents, "Id", "Url", requirementConfirmation.DocumentId);
            ViewBag.RequirementId = new SelectList(db.Requirements, "Id", "Name", requirementConfirmation.RequirementId);
            return View(requirementConfirmation);
        }

        // GET: ReqConfirmations/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReqConfirmation requirementConfirmation = await db.ReqConfirmations.FindAsync(id);
            if (requirementConfirmation == null)
            {
                return HttpNotFound();
            }
            return View(requirementConfirmation);
        }

        // POST: ReqConfirmations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ReqConfirmation requirementConfirmation = await db.ReqConfirmations.FindAsync(id);
            db.ReqConfirmations.Remove(requirementConfirmation);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
