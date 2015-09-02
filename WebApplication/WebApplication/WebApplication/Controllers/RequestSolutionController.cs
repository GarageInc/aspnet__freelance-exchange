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
    using Microsoft.AspNet.Identity;

    public class RequestSolutionController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: RequestSolution
        public async Task<ActionResult> Index()
        {
            var requestSolutions = db.RequestSolutions
                .Include(r => r.Author)
                .Include(r => r.Document);
            return View(await requestSolutions.ToListAsync());
        }

        public async Task<ActionResult> MyIndex()
        {
            var curId = this.User.Identity.GetUserId();
            var requestSolutions = db.RequestSolutions
                .Include(r => r.Author)
                .Include(r => r.Document)
                .Where(r=>r.Author.Id== curId);
            return View(await requestSolutions.ToListAsync());
        }

        // GET: RequestSolution/Details/5
        public async Task<ActionResult> MyDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestSolution requestSolution = await db.RequestSolutions.FindAsync(id);
            if (requestSolution == null)
            {
                return HttpNotFound();
            }
            return View(requestSolution);
        }

        // GET: RequestSolution/Create
        public ActionResult Create()
        {
            var curId = this.User.Identity.GetUserId();
            ViewBag.Requests = new SelectList(db.Requests.Where(x => x.Executor.Id == curId), "Id", "Id");
            return View();
        }

        // POST: RequestSolution/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Comment,ReqId")] RequestSolution requestSolution, HttpPostedFileBase error)
        {
            var curId = this.User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                // если получен файл
                var current = DateTime.Now;
                var user = db.Users.Find(curId);
                if (error != null)
                {
                    Document doc = new Document();
                    doc.Size = error.ContentLength;
                    // Получаем расширение
                    string ext = error.FileName.Substring(error.FileName.LastIndexOf('.'));
                    doc.Type = ext;
                    // сохраняем файл по определенному пути на сервере
                    string path = current.ToString(user.Id.GetHashCode() + "dd/MM/yyyy H:mm:ss").Replace(":", "_").Replace("/", ".") + ext;
                    error.SaveAs(Server.MapPath("~/Files/RequestSolutionFiles/" + path));
                    doc.Url = path;

                    requestSolution.Document = doc;
                    db.Documents.Add(doc);
                }
                else
                    requestSolution.Document = null;

                var req = db.Requests.Find(requestSolution.ReqId);
                requestSolution.Req = req;
                requestSolution.Author = user;
                requestSolution.AuthorId = user.Id;
                
                requestSolution.CreateDateTime = DateTime.Now;

                req.CanDownload = true;
                db.Entry(req).State = EntityState.Modified;

                db.RequestSolutions.Add(requestSolution);

                await db.SaveChangesAsync();
                return RedirectToAction("MyIndex");
            }
            
            ViewBag.Requests = new SelectList(db.Requests.Where(x => x.Executor.Id == curId).Where(x => x.IsPaid), "Id", "Id");
            return View(requestSolution);
        }

        // GET: RequestSolution/Edit/5
        public async Task<ActionResult> MyEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestSolution requestSolution = await db.RequestSolutions.FindAsync(id);
            if (requestSolution == null)
            {
                return HttpNotFound();
            }

            return View(requestSolution);
        }

        // POST: RequestSolution/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MyEdit([Bind(Include = "Id,Name,Comment")] RequestSolution requestSolution, HttpPostedFileBase error)
        {
            if (ModelState.IsValid)
            {
                var reqS = db.RequestSolutions.Find(requestSolution.Id);
                if (error != null)
                {
                    Document doc = new Document();
                    doc.Size = error.ContentLength;
                    // Получаем расширение
                    string ext = error.FileName.Substring(error.FileName.LastIndexOf('.'));
                    doc.Type = ext;
                    // сохраняем файл по определенному пути на сервере
                    string path = DateTime.Now.ToString(this.User.Identity.GetUserId().GetHashCode() + "dd/MM/yyyy H:mm:ss").Replace(":", "_").Replace("/", ".") + ext;
                    error.SaveAs(Server.MapPath("~/Files/RequestSolutionFiles/" + path));
                    doc.Url = path;

                    reqS.Document = doc;
                    db.Documents.Add(doc);
                }

                reqS.Name = requestSolution.Name;
                reqS.Comment = requestSolution.Comment;

                db.Entry(requestSolution).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("MyIndex");
            }

            var curId = this.User.Identity.GetUserId();
            ViewBag.Requests = new SelectList(db.Requests.Where(x => x.Executor.Id == curId), "Id", "Id");
            return View(requestSolution);
        }
     
        // GET: RequestSolution/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestSolution requestSolution = await db.RequestSolutions.FindAsync(id);
            if (requestSolution == null)
            {
                return HttpNotFound();
            }
            return View(requestSolution);
        }

        // POST: RequestSolution/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RequestSolution requestSolution = await db.RequestSolutions.FindAsync(id);
            db.RequestSolutions.Remove(requestSolution);
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
