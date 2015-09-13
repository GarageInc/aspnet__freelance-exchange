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
                .Include(p => p.Request)
                .Include(p => p.RequestSolution)
                .OrderByDescending(p=>p.CreateDateTime);

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
                .Include(p => p.Request)
                .Include(p => p.RequestSolution)
                .OrderByDescending(p => p.CreateDateTime);

            return View(await payments.ToListAsync());
        }
        
        // GET: Payment/Create
        public ActionResult AddingFunds()
        {
            ViewBag.AuthorId = new SelectList(_db.Users, "Id", "Name");

            return View();
        }

        // POST: Payment/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddingFunds([Bind(Include = "Id,Description,Price")] Payment payment, HttpPostedFileBase error)
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
                

                payment.Author = user;
                payment.AuthorId = user.Id;
                payment.IsDeleted = false;
                payment.CreateDateTime = current;
                payment.Checked = false;

                payment.Closed = false;
                payment.AddingFunds = true;
                payment.RequestSolution = null;
                payment.RequestSolutionId = null;
                payment.Request = null;
                payment.RequestId = null;

                _db.Payments.Add(payment);

                await _db.SaveChangesAsync();
                return RedirectToAction("MyIndex");
            }

            ViewBag.AuthorId = new SelectList(_db.Users, "Id", "Name");

            return View(payment);
        }

        
        public ActionResult Create()
        {
            var curId = this.User.Identity.GetUserId();
            
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
        public async Task<ActionResult> Create([Bind(Include = "Id,Description,Price,RequestId")] Payment payment, HttpPostedFileBase error)
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

                var req = _db.Requests.Find(payment.RequestId);
                
                payment.Request = req;
                payment.RequestId = req.Id;
                payment.Author = user;
                payment.AuthorId = user.Id;
                payment.IsDeleted = false;
                payment.CreateDateTime = current;
                payment.Checked = false;

                payment.Closed = false;
                payment.AddingFunds = false;
                payment.RequestSolution = null;
                payment.RequestSolutionId = null;

                _db.Payments.Add(payment);

                await _db.SaveChangesAsync();
                return RedirectToAction("MyIndex");
            }
            
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

            if (Validate(payment) == false)
            {
                return Content("У Вас нет прав на редактирование данной сущности");
            }

            var curId = this.User.Identity.GetUserId();
            
            ViewBag.Requests = new SelectList(_db.Requests
                .Where(x=>x.IsDeleted==false)
                .Where(x => x.Author.Id == curId)
                .Where(x => x.IsPaid == false)
                , "Id", "Id");

            return View(payment);
        }

        private bool Validate(Payment payment)
        {
            if (this.User.IsInRole("Administrator") || User.IsInRole("Moderator") ||
                payment.AuthorId == this.User.Identity.GetUserId())
            {
                return true;
            }
            else
                return false;

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

                if (Validate(pay) == false)
                {
                    return Content("У Вас нет прав на редактирование данной сущности");
                }

                // Запретим CRUD для закрытой оплаты
                if (pay.Closed)
                {
                    return Content("Извините, но нельзя удалить уже закрытую позицию по Оплате");
                }

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

        // Удаление 
        [HttpGet]
        public ActionResult Delete(int id)
        {
            Payment payment = _db.Payments
                .Where(x => x.Id == id)
                .Include(x=>x.Request)
                .First();

            if (Validate(payment) == false)
            {
                return Content("У Вас нет прав на редактирование данной сущности");
            }

            if (payment != null)
            {
                return PartialView("_Delete", payment);
            }
            return View("Index");
        }
        

        // Удаление 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public void DeleteConfirmed(int id)
        {
            Payment payment = _db.Payments.Find(id);
            var curId = HttpContext.User.Identity.GetUserId();

            if (Validate(payment) == false)
            {
                return;// Content("У Вас нет прав на редактирование данной сущности");
            }

            // получаем текущего пользователя
            ApplicationUser user = _db.Users.First(m => m.Id == curId);

            // Запретим CRUD для закрытой оплаты
            if (payment.Closed)
            {
                Content("Извините, но нельзя удалить уже закрытую позицию по Оплате");
            }

            // Если автор совпадает и оплата не проверена администрацией - её ещё удалить можно.
            if (payment != null && payment.Author.Id == user.Id && !payment.Checked)
            {
                // Изменим сразу же и заявку, за которую отвечает оплата
                var request = _db.Requests.Find(payment.RequestId);
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
        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult PaymentChangeStatus(int? paymentId, int status)
        {
            var curId = HttpContext.User.Identity.GetUserId();
            ApplicationUser userAuth = _db.Users.First(m => m.Id == curId);

            if (userAuth == null)
            {
                return RedirectToAction("LogOff", "Account");
            }
            
            Payment pay = _db.Payments.Find(paymentId);
            if (pay != null)
            {
                // Берем того, кто отправил заявку
                ApplicationUser author = _db.Users.First(m => m.Id == pay.AuthorId);

                // Если это простой перевод
                if (pay.RequestId == null)
                {

                    switch (status)
                    {
                        case 0:
                            {
                                // Т.о. нельзя "опустить" в минус плательщика
                                if (pay.Checked == true)
                                {

                                    // Оплата - не проверена
                                    pay.Checked = false;

                                    // Выводим на счет пользователя
                                    author.Balance -= pay.Price;
                                    _db.Entry(author).State = EntityState.Modified;
                                    _db.SaveChanges();
                                }

                                break;
                            }

                        case 1:
                            {
                                // Данное действие в любом случае будет первым
                                if (pay.Checked == false)
                                {
                                    pay.Checked = true;

                                    author.Balance += pay.Price;
                                    _db.Entry(author).State = EntityState.Modified;
                                    _db.SaveChanges();
                                }

                                break;
                            }
                    }
                }
                // Иначе это оплата за задачу
                else
                {
                    var req = _db.Requests.Find(pay.Request.Id);

                    if(pay.AuthorId!=req.AuthorId)
                        return Content("Авторы оплаты и заявки не совпадают! Что-то тут не то, товарищи, кто-то нас хочет сломать. Зовите разработчика логики");

                    switch (status)
                    {
                        case 0:
                            {
                                // Т.о. нельзя "опустить" в минус плательщика
                                if (pay.Checked == true)
                                {
                                    // Оплата - не проверена
                                    pay.Checked = false;
                                    // Т.е. заявка - не оплачена
                                    req.IsPaid = false;

                                    // Выводим на счет пользователя
                                    author.Balance -= pay.Price;
                                    _db.Entry(author).State = EntityState.Modified;
                                    _db.SaveChanges();
                                }

                                break;
                            }

                        case 1:
                            {
                                // Данное действие в любом случае будет первым
                                if (pay.Checked == false)
                                {
                                    pay.Checked = true;

                                    req.IsPaid = true;

                                    author.Balance += pay.Price;
                                    _db.Entry(author).State = EntityState.Modified;
                                    _db.SaveChanges();
                                }

                                break;
                            }
                    }
                    _db.Entry(req).State = EntityState.Modified;
                }
            }

            _db.Entry(pay).State = EntityState.Modified;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Подтверждение перевода
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult Remittance(int id)
        {
            Payment payment = _db.Payments
                .Where(x => x.Id == id)
                .Include(x => x.Request)
                .First();

            Request request = _db.Requests.Find(payment.RequestId);
            if (payment != null)
            {
                ViewBag.PaymentId = payment.Id;
                ViewBag.Closed = payment.Closed;
                return PartialView("_Remittance", request);
            }
            return View("Index");
        }

        [HttpPost, ActionName("Remittance")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult RemittanceConfirmed(int requestId, int paymentId)
        {
            Request request = _db.Requests.Find(requestId);
            Payment payment = _db.Payments.Find(paymentId);
            
            // получаем текущего пользователя
            ApplicationUser author = _db.Users.First(m => m.Id == request.AuthorId);
            ApplicationUser executor = _db.Users.First(m => m.Id == request.ExecutorId);

            if (request.Executor == null)
            {
                return Content("Не установлен исполнитель, потому перевод пока что невозможен. Выберите исполнителя задачи и пусть он загрузит решение");
            }

            // Снова проверим - можно ли переводить?
            if (payment.Closed == false)
            {
                if (request.Price > author.Balance)
                {
                    return Content("Ошибка! Стоимость задачи ВЫШЕ баланса автора, полный перевод на счет исполнителя не возможен!");
                }
                else
                {
                    // Проведем перевод
                    executor.Balance += request.Price;
                    author.Balance -= request.Price;

                    // Объявим операцию перевода закрытой
                    payment.Closed = true;

                    // Пишем, что решения могут быть скачаны

                    // Делаем так, что решение можно загружать.
                    var req = _db.Requests.Find(payment.RequestId);
                    req.CanDownload = true;
                    _db.Entry(req).State = EntityState.Modified;

                    // Сохраним изменения в базе данных
                    _db.Entry(author).State=EntityState.Modified;
                    _db.Entry(executor).State=EntityState.Modified;

                    _db.SaveChanges();
                }

                return View("Index");
            }

            return Content("Ошибка! Данная позиция(оплата) уже закрыта!");
        }

        /// <summary>
        /// Закрытие заявки по переводу
        /// </summary>
        /// <param name="id"></param>
        [Authorize(Roles = "Administrator, Moderator")]
        public void ClosePayment(string id)
        {
            var payId = int.Parse(id);
            Payment payment = _db.Payments.Find(payId);

            payment.Closed = true;

            _db.Entry(payment).State = EntityState.Modified;
            _db.SaveChanges();
            
            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }
    }
}
