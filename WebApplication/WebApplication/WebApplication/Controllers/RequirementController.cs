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
    public class RequirementController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Requirement
        public async Task<ActionResult> Index()
        {
            var requirements = db.Requirements.Include(r => r.Author).Include(r => r.Document);
            return View(await requirements.ToListAsync());
        }

        // GET: Requirement/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Requirement requirement = await db.Requirements.FindAsync(id);
            if (requirement == null)
            {
                return HttpNotFound();
            }
            return View(requirement);
        }

        // GET: Requirement/Create
        public ActionResult Create()
        {
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "Name");
            ViewBag.DocumentId = new SelectList(db.Documents, "Id", "Url");
            return View();
        }

        // POST: Requirement/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Description,DocumentId,AuthorId,Status,Blocked,BlockedReason,Price")] Requirement requirement)
        {
            if (ModelState.IsValid)
            {
                db.Requirements.Add(requirement);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.AuthorId = new SelectList(db.Users, "Id", "Name", requirement.AuthorId);
            ViewBag.DocumentId = new SelectList(db.Documents, "Id", "Url", requirement.DocumentId);
            return View(requirement);
        }

        // GET: Requirement/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Requirement requirement = await db.Requirements.FindAsync(id);
            if (requirement == null)
            {
                return HttpNotFound();
            }
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "Name", requirement.AuthorId);
            ViewBag.DocumentId = new SelectList(db.Documents, "Id", "Url", requirement.DocumentId);
            return View(requirement);
        }

        // POST: Requirement/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,DocumentId,AuthorId,Status,Blocked,BlockedReason,Price")] Requirement requirement)
        {
            if (ModelState.IsValid)
            {
                db.Entry(requirement).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "Name", requirement.AuthorId);
            ViewBag.DocumentId = new SelectList(db.Documents, "Id", "Url", requirement.DocumentId);
            return View(requirement);
        }

        // GET: Requirement/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Requirement requirement = await db.Requirements.FindAsync(id);
            if (requirement == null)
            {
                return HttpNotFound();
            }
            return View(requirement);
        }

        // POST: Requirement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Requirement requirement = await db.Requirements.FindAsync(id);
            db.Requirements.Remove(requirement);
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
