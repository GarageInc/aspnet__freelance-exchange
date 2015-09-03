namespace WebApplication.Controllers
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;
    using WebApplication.Models;
    using Microsoft.AspNet.Identity;
    using WebApplication.Service;

    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly DocumentService _docService = new DocumentService();

        // GET: Payment
        public async Task<ActionResult> Index()
        {
            var payments = _db.Payments
                .Where(p=>p.IsDeleted==false)
                .Include(p => p.Author)
                .Include(p => p.Document)
                .Include(p => p.Req)
                .Include(p => p.ReqSolution);

            return View(await payments.ToListAsync());
        }

        public async Task<ActionResult> MyIndex()
        {
            var curId = this.User.Identity.GetUserId();

            var payments = _db.Payments
                .Where(p => p.IsDeleted == false)
                .Where(p => p.Author.Id == curId)
                .Include(p => p.Author)
                .Include(p => p.Document)
                .Include(p => p.Req)
                .Include(p => p.ReqSolution);

            return View(await payments.ToListAsync());
        }

        // GET: Payment/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = await _db.Payments.FindAsync(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // GET: Payment/Create
        public ActionResult Create()
        {
            var curId = this.User.Identity.GetUserId();

            ViewBag.AuthorId = new SelectList(_db.Users, "Id", "Name");
            ViewBag.Requests = new SelectList(_db.Requests
                .Where(x=>x.Author.Id==curId)
                .Where(x=>x.IsPaid==false)
                .Where(x=>x.IsDeleted==false)
                , "Id", "Id");

            return View();
        }

        // POST: Payment/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Description,Price,ReqId")] Payment payment, HttpPostedFileBase error)
        {
            var curId = this.User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                // если получен файл
                var current = DateTime.Now;
                var user = _db.Users.Find(curId);
                if (error != null)
                {
                    payment.Document = _docService.CreateDocument(Server.MapPath("~/Files/PaymentFiles/"), error);
                }
                else
                    payment.Document = null;

                var req = _db.Requests.Find(payment.ReqId);
                
                payment.Req = req;
                payment.Author = user;
                payment.AuthorId = user.Id;
                payment.IsDeleted = false;
                payment.CreateDateTime = current;
                payment.Checked = false;

                payment.ReqSolution = null;
                payment.ReqSolutionId = null;

                _db.Payments.Add(payment);

                await _db.SaveChangesAsync();
                return RedirectToAction("MyIndex");
            }
            
            ViewBag.AuthorId = new SelectList(_db.Users, "Id", "Name");
            ViewBag.Requests = new SelectList(_db.Requests
                .Where(x => x.Author.Id == curId)
                .Where(x => x.IsPaid == false)
                .Where(x => x.IsDeleted == false)
                , "Id", "Id");

            return View(payment);
        }
        
        /// <summary>
        /// Редактирование
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = await _db.Payments.FindAsync(id);
            if (payment == null)
            {
                return HttpNotFound();
            }

            var curId = this.User.Identity.GetUserId();
            ViewBag.AuthorId = new SelectList(_db.Users, "Id", "Name");
            ViewBag.Requests = new SelectList(_db.Requests
                .Where(x=>x.IsDeleted==false)
                .Where(x => x.Author.Id == curId)
                .Where(x => x.IsPaid == false)
                , "Id", "Id");

            return View(payment);
        }
        
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Description,Price")] Payment payment, HttpPostedFileBase error)
        {
            if (ModelState.IsValid)
            {
                var pay = _db.Payments.Find(payment.Id);
                if (error != null)
                {
                    pay.Document = _docService.CreateDocument(Server.MapPath("~/Files/PaymentFiles/"), error);
                }

                pay.Description = payment.Description;
                pay.Price = payment.Price;

                _db.Entry(pay).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("MyIndex");
            }

            var curId = this.User.Identity.GetUserId();
            ViewBag.AuthorId = new SelectList(_db.Users, "Id", "Name");
            ViewBag.Requests = new SelectList(_db.Requests
                .Where(x => x.IsDeleted == false)
                .Where(x => x.Author.Id == curId)
                .Where(x => x.IsPaid == false)
                , "Id", "Id");
            return View(payment);
        }

        // Удаление заявки
        [HttpGet]
        public ActionResult Delete(int id)
        {
            Payment payment = _db.Payments
                .Where(x => x.Id == id)
                .Include(x=>x.Req)
                .First();

            if (payment != null)
            {
                return PartialView("_Delete", payment);
            }
            return View("Index");
        }


        // Удаление заявки
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public void DeleteConfirmed(int id)
        {
            Payment payment = _db.Payments.Find(id);
            var curId = HttpContext.User.Identity.GetUserId();
            // получаем текущего пользователя
            ApplicationUser user = _db.Users.First(m => m.Id == curId);
            if (payment != null && payment.Author.Id == user.Id)
            {
                // Изменим сразу же и заявку, за которую отвечает оплата
                var request = _db.Requests.Find(payment.ReqId);
                request.IsPaid = false;
                request.CanDownload = false;

                payment.IsDeleted = true;

                _db.Entry(request).State = EntityState.Modified;
                _db.Entry(payment).State = EntityState.Modified;

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

        /// <summary>
        /// Оплаченность изменяется
        /// </summary>
        /// <param name="paymentId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]        
        public ActionResult PaymentChangeStatus(int? paymentId, int status)
        {
            var curId = HttpContext.User.Identity.GetUserId();
            ApplicationUser user = _db.Users.First(m => m.Id == curId);

            if (user == null)
            {
                return RedirectToAction("LogOff", "Account");
            }

            Payment pay = _db.Payments.Find(paymentId);
            if (pay != null)
            {
                // Найдем заявку, которую регулирует эта оплата
                var req = _db.Requests.Find(pay.Req.Id);
                switch (status)
                {
                    case 0:
                        pay.Checked = false;

                        req.IsPaid = false;
                        req.CanDownload = false;
                        break;
                    case 1:
                        pay.Checked = true;

                        req.IsPaid = true;
                        req.CanDownload = true;
                        break;
                }

                _db.Entry(req).State = EntityState.Modified;
                _db.Entry(pay).State = EntityState.Modified;
                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
