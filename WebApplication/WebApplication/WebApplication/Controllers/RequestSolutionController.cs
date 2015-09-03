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
    using Microsoft.AspNet.Identity;

    [Authorize]
    public class RequestSolutionController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly DocumentService _docService = new DocumentService();

        // GET: RequestSolution
        public async Task<ActionResult> Index()
        {
            var requestSolutions = _db.RequestSolutions
                .Where(r=>!r.IsDeleted)
                .Include(r => r.Author)
                .Include(r => r.Document);

            return View(await requestSolutions.ToListAsync());
        }

        public async Task<ActionResult> MyIndex()
        {
            var curId = this.User.Identity.GetUserId();
            var requestSolutions = _db.RequestSolutions
                .Where(r => !r.IsDeleted)
                .Where(r => r.Author.Id == curId)
                .Include(r => r.Author)
                .Include(r => r.Document);

            return View(await requestSolutions.ToListAsync());
        }
        
        // GET: RequestSolution/Create
        public ActionResult Create()
        {
            var curId = this.User.Identity.GetUserId();

            ViewBag.Requests = new SelectList(_db.Requests
                .Where(x => x.Executor.Id == curId)
                .Where(x=>!x.IsDeleted)
                , "Id", "Id");

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
                var user = _db.Users.Find(curId);
                if (error != null)
                {
                    requestSolution.Document = _docService.CreateDocument(Server.MapPath("~/Files/RequestSolutionFiles/"), error);
                }
                else
                    requestSolution.Document = null;

                var req = _db.Requests.Find(requestSolution.ReqId);
                requestSolution.Req = req;
                requestSolution.ReqId = req.Id;
                requestSolution.Author = user;
                requestSolution.AuthorId = user.Id;
                requestSolution.IsDeleted = false;
                requestSolution.CreateDateTime = DateTime.Now;
                

                _db.RequestSolutions.Add(requestSolution);

                await _db.SaveChangesAsync();
                return RedirectToAction("MyIndex");
            }

            ViewBag.Requests = new SelectList(_db.Requests
                 .Where(x => x.Executor.Id == curId)
                 .Where(x => !x.IsDeleted)
                 , "Id", "Id");

            return View(requestSolution);
        }

        // GET: RequestSolution/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestSolution requestSolution = await _db.RequestSolutions.FindAsync(id);
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
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Comment")] RequestSolution requestSolution, HttpPostedFileBase error)
        {
            if (ModelState.IsValid)
            {
                var reqS = _db.RequestSolutions.Find(requestSolution.Id);
                if (error != null)
                {

                    reqS.Document = _docService.CreateDocument(Server.MapPath("~/Files/RequestSolutionFiles/"), error);
                }

                reqS.Name = requestSolution.Name;
                reqS.Comment = requestSolution.Comment;

                _db.Entry(requestSolution).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("MyIndex");
            }

            var curId = this.User.Identity.GetUserId();
            ViewBag.Requests = new SelectList(_db.Requests
                .Where(x => x.Executor.Id == curId)
                .Where(x => !x.IsDeleted),
                "Id", "Id");

            return View(requestSolution);
        }

        // Удаление заявки
        [HttpGet]
        public ActionResult Delete(int id)
        {
            RequestSolution requestSol = _db.RequestSolutions
                .Where(x => x.Id == id)
                .Where(x=>x.IsDeleted==false)
                .Include(x => x.Author)
                .First();

            if (requestSol != null)
            {
                //получаем категорию
                return PartialView("_Delete", requestSol);
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
