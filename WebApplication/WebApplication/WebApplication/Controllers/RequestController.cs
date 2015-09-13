namespace WebApplication.Controllers
{
    using System.Data;
    using System;
    using System.Collections.Generic;
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
    using System.Transactions;

    [Authorize]// Только авторизованным
    public class RequestController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly DocumentService _docService = new DocumentService();

        // Вывод всех задач по выбранному критерию
        [AllowAnonymous]// Исключение - могут смотреть все
        public ActionResult Index(int? category)
        {
            IEnumerable<Request> allReqs = null;
            if (category == null || category == 0)
            {
                allReqs = _db.Requests
                                        .Where(r => !r.IsDeleted)
                                        .Include(r => r.Category)  // добавляем категории
                                        .Include(r => r.Lifecycle)  // добавляем жизненный цикл заявок
                                        .Include(r => r.Author) // добавляем данные о пользователях
                                        .Include(r => r.Solvers)
                                        .Include(r=>r.Subject)
                                        .OrderByDescending(r => r.Lifecycle.Opened);
            }
            else
                allReqs = _db.Requests
                                        .Where(x => x.CategoryId == category)
                                        .Where(r => !r.IsDeleted)
                                        .Include(r => r.Category)  // добавляем категории
                                        .Include(r => r.Lifecycle)  // добавляем жизненный цикл заявок
                                        .Include(r => r.Author)         // добавляем данные о пользователях
                                        .Include(r => r.Solvers)
                                        .Include(r=>r.Subject)
                                        .OrderByDescending(r => r.Lifecycle.Opened);
            
            List<Category> categories = _db.Categories.ToList();

            //Добавляем в список возможность выбора всех
            categories.Insert(0, new Category { Name = "Все", Id = 0 });
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View(allReqs.ToList());
        }


        /// <summary>
        /// Получение заявок текущего пользователя
        /// </summary>
        /// <returns></returns>
        public ActionResult MyIndex(int? category)
        {
            var currentId = HttpContext.User.Identity.GetUserId();
            // получаем текущего пользователя
            ApplicationUser user = _db.Users.FirstOrDefault(m => m.Id == currentId);
            IEnumerable<Request> allReqs = null;
            if (category == null || category == 0)
            {

                allReqs = _db.Requests.Where(r => r.Author.Id == user.Id)
                                        .Where(r => !r.IsDeleted)
                                        .Include(r => r.Category)  // добавляем категории
                                        .Include(r => r.Lifecycle)  // добавляем жизненный цикл заявок
                                        .Include(r => r.Author)         // добавляем данные о пользователях
                                        .Include(r => r.Solvers)
                                        .Include(r => r.Subject)
                                        .OrderByDescending(r => r.Lifecycle.Opened); // упорядочиваем по дате по убыванию   
            }
            else
                allReqs = _db.Requests
                                        .Where(x => x.CategoryId == category)
                                        .Where(r => r.Author.Id == user.Id)
                                        .Where(r => !r.IsDeleted) //получаем заявки для текущего пользователя
                                        .Include(r => r.Category)  // добавляем категории
                                        .Include(r => r.Lifecycle)  // добавляем жизненный цикл заявок
                                        .Include(r => r.Author)         // добавляем данные о пользователях
                                        .Include(r => r.Solvers)
                                        .Include(r => r.Subject)
                                        .OrderByDescending(r => r.Lifecycle.Opened); // упорядочиваем по дате по убыванию  ;


            List<Category> categories = _db.Categories.ToList();

            //Добавляем в список возможность выбора всех
            categories.Insert(0, new Category { Name = "Все", Id = 0 });
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View(allReqs.ToList());
        }

        // Редактировать заявку
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Request request = await _db.Requests.FindAsync(id);
            if (request == null)
            {
                return HttpNotFound();
            }

            var payments = _db.Payments
                                .Where(x => x.RequestId == request.Id)
                                .Where(x => x.Closed);
            if (payments.Any())
            {

                return PartialView("_Content", "Извините, но оплата за данную задачу уже утверждена и заявка не может быть изменена!");
            }

            // Добавляются категории и предмет
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");
            ViewBag.Subjects = new SelectList(_db.Subjects, "Id", "Name");
            return PartialView(request);
        }

        // POST: Request/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,CategoryId,SubjectId,Deadline,Priority,Price")] Request request, HttpPostedFileBase error)
        {
            var curId = this.HttpContext.User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                try
                {
                    var requestBase = _db.Requests.Find(request.Id);
                    if (requestBase.Author.Id == curId)
                    {
                        using (new TransactionScope(TransactionScopeOption.Required,
                                new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                        {
                            // Проверим, если оплата по этой задаче уже проверена администрацией и закрыта - то увы, изменить ничего нельзя
                            var payments = _db.Payments
                                .Where(x => x.RequestId == request.Id)
                                .Where(x => x.Closed);
                            if (payments.Any())
                            {

                                return PartialView("_Content", "Извините, но оплата за данную задачу уже утверждена и заявка не может быть изменена!");
                            }

                            if (error != null)
                            {
                                requestBase.Document = _docService.CreateDocument(Server.MapPath("~/Files/RequestFiles/"), error);
                            }

                            requestBase.Name = request.Name;

                            requestBase.Category = _db.Categories.Find(request.CategoryId);
                            requestBase.CategoryId = requestBase.Category.Id;

                            requestBase.Subject = _db.Subjects.Find(request.SubjectId);
                            requestBase.SubjectId = requestBase.Subject.Id;

                            requestBase.Deadline = request.Deadline;
                            requestBase.Priority = request.Priority;
                            requestBase.Description = request.Description;
                            requestBase.Price = request.Price;

                            _db.Entry(requestBase).State = EntityState.Modified;

                        }

                        await _db.SaveChangesAsync();
                        return RedirectToAction("MyIndex");
                    }
                    else
                    {
                        return PartialView("_Content", "К сожалению, Вы не автор данной задачи и редактировать её не можете");
                    }
                }
                catch (Exception e)
                {
                    return PartialView("_Content", e.Message);
                }
            }

            ViewBag.AuthorId = new SelectList(_db.Users, "Id", "Name");
            ViewBag.Requests = new SelectList(_db.Requests.Where(x => x.Author.Id == curId).Where(x => x.IsPaid == false), "Id", "Id");
            return View(request);
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
                // Добавляются категории и предмет
                ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");
                ViewBag.Subjects = new SelectList(_db.Subjects, "Id", "Name");

                return View();
            }
            return RedirectToAction("LogOff", "Account");
        }

        // Создание новой заявки
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Request request, HttpPostedFileBase error)
        {
            var curId = HttpContext.User.Identity.GetUserId();
            // получаем текущего пользователя
            ApplicationUser user = _db.Users.FirstOrDefault(m => m.Id == curId);
            if (user == null)
            {
                return RedirectToAction("LogOff", "Account");
            }
            if (ModelState.IsValid)
            {
                //получаем время открытия
                DateTime current = DateTime.Now;
                //Создаем запись о жизненном цикле заявки
                Lifecycle newLifecycle = new Lifecycle() { Opened = current, IsDeleted = false, CreateDateTime = current};
                request.Lifecycle = newLifecycle;
                request.LifecycleId = newLifecycle.Id;

                //Добавляем жизненный цикл заявки
                _db.Lifecycles.Add(newLifecycle);

                // указываем пользователя заявки
                request.Author = user;
                request.AuthorId = user.Id;
                
                // если получен файл
                if (error != null)
                {
                    request.Document = _docService.CreateDocument(Server.MapPath("~/Files/RequestFiles/"), error);
                }
                else
                    request.Document = null;

                var cat = _db.Categories.Find(request.CategoryId);
                request.Category = cat;
                var sub = _db.Subjects.Find(request.SubjectId);
                request.Subject = sub;
                request.CreateDateTime = DateTime.Now;
                request.IsDeleted = false;
                request.Checked = false;
                request.CanDownload = false;
                
                // указываем статус Открыта у заявки
                request.IsPaid = false;
                request.Status = (int)RequestStatus.Open;

                // Добавляем заявку с возможно приложенными документами
                _db.Requests.Add(request);
                user.Requests.Add(request);
                _db.Entry(user).State = EntityState.Modified;

                try
                {
                    _db.SaveChanges();
                }
                catch(Exception e)
                {
                    return PartialView("_Content",e.Message);
                }

                return RedirectToAction("Index");
            }

            // Добавляются категории и предмет
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");
            ViewBag.Subjects = new SelectList(_db.Subjects, "Id", "Name");
            return View(request);
        }


        /// <summary>
        /// Просмотр подробных сведений о заявке
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            Request request = _db.Requests
                .Where(x=>x.Id==id)
                .Include(x=>x.Subject)
                .Include(x => x.Category)
                .First();

            if (request != null)
            {
                //получаем категорию
                request.Category = _db.Categories.First(m => m.Id == request.CategoryId);
                return PartialView("_Details", request);
            }
            return View("Index");
        }


        [AllowAnonymous]
        public ActionResult Lifecycle(int id)
        {
            Lifecycle lifecycle = _db.Lifecycles.First(m => m.Id == id);

            if (lifecycle != null)
            {
                return PartialView("_Lifecycle", lifecycle);
            }
            return View("Index");
        }
        
        // Удаление заявки
        [HttpGet]
        public ActionResult Delete(int id)
        {
            Request request = _db.Requests
                .Where(x => x.Id == id)
                .Include(x => x.Subject)
                .Include(x => x.Category)
                .First();

            if (request != null)
            {
                // Проверим, если оплата по этой задаче уже проверена администрацией и закрыта - то увы, изменить ничего нельзя
                var payments = _db.Payments
                    .Where(x => x.RequestId == request.Id)
                    .Where(x => x.Closed);

                if (payments.Any())
                {
                    return PartialView("_Content","Извините, но оплата за данную задачу уже утверждена и заявка не может быть удалена!");
                }

                //получаем категорию
                request.Category = _db.Categories.First(m => m.Id == request.CategoryId);
                return PartialView("_Delete", request);
            }
            return View("Index");
        }


        // Удаление заявки
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public void DeleteConfirmed(int id)
        {
            Request request = _db.Requests.Find(id);
            var curId = HttpContext.User.Identity.GetUserId();
            // получаем текущего пользователя
            ApplicationUser user = _db.Users.First(m => m.Id == curId);
            if (request != null && request.Author.Id == user.Id)
            {
                Lifecycle lifecycle = _db.Lifecycles.Find(request.LifecycleId);
                lifecycle.IsDeleted = true;
                request.IsDeleted = true;
                request.DateOfDeleting=DateTime.Now;

                _db.Entry(lifecycle).State = EntityState.Modified;
                _db.Entry(request).State = EntityState.Modified;

                _db.SaveChanges();
            }
            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        /// <summary>
        /// Редактирование заявок - назначение исполнителей
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Moderator, Administrator")]
        public ActionResult Distribute(int? category)
        {
            IEnumerable<Request> requests = null;
            if (category == null || category == 0)
            {
                requests=_db.Requests
                    .Where(r=>r.IsDeleted==false)
                    .Include(r => r.Author)
                    .Include(r => r.Lifecycle)
                    .Include(r => r.Executor)
                    .Include(r => r.Document)
                    .Include(r => r.Subject)
                    .Include(r => r.Solvers);
            }
            else
                requests = _db.Requests
                    .Where(r => r.IsDeleted == false)
                    .Where(x => x.CategoryId == category)
                    .Include(r => r.Author)
                    .Include(r => r.Lifecycle)
                    .Include(r => r.Executor)
                    .Include(r => r.Document)
                    .Include(r => r.Subject)
                    .Include(r=>r.Solvers);

            var curId = this.User.Identity.GetUserId();
            var users = _db.Users;

            var executors = users.ToList();// Думаю, пока не стоит делать только для обычных пользователей
            ViewBag.Executors = new SelectList(executors, "Id", "Name");

            List<Category> categories = _db.Categories.ToList();

            //Добавляем в список возможность выбора всех
            categories.Insert(0, new Category { Name = "Все", Id = 0 });
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View("Distribute",requests);
        }
        
        [Authorize(Roles = "Moderator, Administrator")]
        public ActionResult DistributeRequest(int? requestId, string Id)
        {
            if (requestId == null && Id.IsEmpty())// == null)
            {
                return RedirectToAction("Distribute");
            }

            Request req = _db.Requests.Find(requestId);
            ApplicationUser ex = _db.Users.Find(Id);
            if (req == null || ex == null)
            {
                return RedirectToAction("Distribute");
            }

            // Проверим, если оплата по этой задаче уже проверена администрацией и закрыта - то увы, изменить ничего нельзя
            var payments = _db.Payments
                .Where(x => x.RequestId == requestId)
                .Where(x=>x.Closed);
            if(payments.Any())
            {
                return PartialView("_Content","Извините, но оплата за данную задачу уже утверждена и пользователь не может быть изменён!");
            }

            req.ExecutorId = Id;
            req.Executor = ex;

            req.Status = (int)RequestStatus.Distributed;
            Lifecycle lifecycle = _db.Lifecycles.Find(req.LifecycleId);
            lifecycle.Distributed = DateTime.Now;
            _db.Entry(lifecycle).State = EntityState.Modified;

            _db.Entry(req).State = EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("Distribute");
        }


        //Заявки для изменения статуса исполнителем
        [HttpGet]
        public ActionResult ChangeStatus()
        {
            // получаем текущего пользователя
            var curId = HttpContext.User.Identity.GetUserId();
            ApplicationUser user = _db.Users.FirstOrDefault(m => m.Id == curId);

            if (user != null)
            {
                var requests = _db.Requests
                    .Include(r => r.Author)
                    .Include(r => r.Lifecycle)
                    .Include(r => r.Executor)
                    .Include(r => r.Document)
                    .Include(r => r.Category)
                    .Include(r => r.Subject)
                    .Where(r => r.ExecutorId == user.Id);

                return View(requests);
            }

            return RedirectToAction("LogOff", "Account");
        }

        [HttpPost]
        public ActionResult ChangeStatus(int requestId, int status)
        {
            var curId = HttpContext.User.Identity.GetUserId();
            ApplicationUser user = _db.Users.First(m => m.Id == curId);

            if (user == null)
            {
                return RedirectToAction("LogOff", "Account");
            }

            Request req = _db.Requests.Find(requestId);
            if (req.Executor.Id != curId)
            {
                return PartialView("_Content","К сожалению, Вы не исполнитель данной заявки и не можете изменить её статус");
            }

            if (req != null)
            {
                req.Status = status;

                Lifecycle lifecycle = _db.Lifecycles.Find(req.LifecycleId);
                if (status == (int)RequestStatus.Open)
                {
                    lifecycle.Opened = DateTime.Now;
                    lifecycle.Distributed = null;
                    lifecycle.Proccesing = null;
                    lifecycle.Checking = null;
                    lifecycle.Closed = null;
                }
                if (status == (int)RequestStatus.Distributed)
                {
                    lifecycle.Distributed = DateTime.Now;
                    lifecycle.Proccesing = null;
                    lifecycle.Checking = null;
                    lifecycle.Closed = null;
                }
                if (status == (int)RequestStatus.Proccesing)
                {
                    lifecycle.Proccesing = DateTime.Now;
                    lifecycle.Checking = null;
                    lifecycle.Closed = null;
                }
                else if (status == (int)RequestStatus.Closed)
                {
                    lifecycle.Closed = DateTime.Now;
                    lifecycle.Closed = null;
                }
                _db.Entry(lifecycle).State = EntityState.Modified;
                _db.Entry(req).State = EntityState.Modified;
                _db.SaveChanges();
            }

            return RedirectToAction("ChangeStatus");
        }

        /// <summary>
        /// Статистика
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetCountOfAllRequests()
        {
            string count = _db.Requests.Count(x=>!x.IsDeleted).ToString();
            ViewBag.Message = count;
            return PartialView("Message");
        }
        [AllowAnonymous]
        public ActionResult GetCountOfAllSolvedRequests()
        {
            string count = _db.RequestSolutions.Count(x => !x.IsDeleted).ToString();
            ViewBag.Message = count;
            return PartialView("Message");
        }
        [AllowAnonymous]
        public ActionResult GetCountOfAllUsers()
        {
            string count = _db.Users.Count().ToString();
            ViewBag.Message = count;
            return PartialView("Message");
        }

        /// <summary>
        /// Изменение проверки администрацией
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Moderator, Administrator")]
        public ActionResult DistributeChangeStatus(int requestId, int status)
        {
            var curId = HttpContext.User.Identity.GetUserId();
            ApplicationUser user = _db.Users.First(m => m.Id == curId);

            if (user == null)
            {
                return RedirectToAction("LogOff", "Account");
            }

            Request req = _db.Requests.Find(requestId);
            if (req != null)
            {
                if (status == 0)
                {
                    req.Checked = false;
                    req.Executor = null;
                    req.ExecutorId = null;
                }
                else if (status == 1)
                {
                    req.Checked = true;
                }
                
                _db.Entry(req).State = EntityState.Modified;
                _db.SaveChanges();
            }

            return RedirectToAction("Distribute");
        }
        
        /// <summary>
        /// Нажатие на кнопку "Хочу решать"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddToSolvers(int id)
        {
            var req = _db.Requests.Find(id);
            var curId = this.HttpContext.User.Identity.GetUserId();
            var thisUser = _db.Users.Find(curId);

            if (req.AuthorId == curId)
                return PartialView("_Content","Извините, но добавиться к решению собственной задачи нельзя");

            if (req.Solvers.Count(x => x.Id == curId) == 0)
            {
                req.Solvers.Add(thisUser);
            }

            _db.Entry(req).State = EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Пользователь сам устанавливает исполнителя
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="executorId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MySelfDistribute(int? requestId, string executorId)
        {
            if (requestId == null && executorId.IsEmpty())// == null)
            {
                return RedirectToAction("MyIndex");
            }

            Request req = _db.Requests.Find(requestId);

            // Сделаем защиту решателя: нельзя изменить решателя в последний момент, чтобы оплата за задачу перешла ему, в то время как настоящий исполнитель ничего не получит
            
            // Если уже назначен пользователь - проверим, загружена ли оплата уже проверенная по этой задаче.
            // Если оплата присутствует, то она фиксируется как за труды данного решателя и поэтому другой решатель не может быть изменён
            if (req.ExecutorId != null)
            {
                var allPayments = _db.Payments
                .Where(x => x.IsDeleted == false)
                .Where(x => x.Checked)
                .Where(x => x.RequestId == requestId)
                .Include(x => x.Request);

                if (allPayments.Any())
                {
                    return
                        PartialView("_Content",
                            "К сожалению, решатель уже установлен и оплата за задачу загружена Вами, Вы не можете изменить решателя"
                            );
                }
            }

            ApplicationUser ex = _db.Users.Find(executorId);

            if (req == null || ex == null)
            {
                return RedirectToAction("MyIndex");
            }

            // Проверим, если оплата по этой задаче уже проверена администрацией и закрыта - то увы, изменить ничего нельзя
            var payments = _db.Payments
                .Where(x => x.RequestId == requestId)
                .Where(x => x.Closed);
            if (payments.Any())
            {
                return PartialView("_Content","Извините, но оплата за данную задачу уже утверждена и пользователь не может быть изменён!");
            }

            req.ExecutorId = executorId;
            req.Executor = ex;
            req.Status = (int)RequestStatus.Distributed;
            Lifecycle lifecycle = _db.Lifecycles.Find(req.LifecycleId);
            lifecycle.Distributed = DateTime.Now;
            _db.Entry(lifecycle).State = EntityState.Modified;

            _db.Entry(req).State = EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("MyIndex");
        }

        /// <summary>
        /// Получение все решений по данной заявке
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetAllSolutions(int id)
        {
            var reqSols = _db.RequestSolutions
                .Where(x => x.IsDeleted == false)
                .Where(x => x.RequestId == id)
                .Include(x=>x.Author);

            return PartialView("_GetAllSolutions", reqSols);
        }
    }
}
