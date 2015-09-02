namespace WebApplication.Controllers
{
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

    [Authorize]// Только авторизованным
    public class RequestController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // Вывод всех задач по выбранному критерию
        [AllowAnonymous]// Исключение - могут смотреть все
        public ActionResult Index(int? category)
        {
            IEnumerable<Request> allReqs = null;
            if (category == null || category == 0)
            {
                allReqs = _db.Requests
                                        .Include(r => r.Category)  // добавляем категории
                                        .Include(r => r.Lifecycle)  // добавляем жизненный цикл заявок
                                        .Include(r => r.Author) // добавляем данные о пользователях
                                        .Include(r => r.Solvers);
            }
            else
                allReqs = _db.Requests.Where(x => x.CategoryId == category)
                                        .Include(r => r.Category)  // добавляем категории
                                        .Include(r => r.Lifecycle)  // добавляем жизненный цикл заявок
                                        .Include(r => r.Author)         // добавляем данные о пользователях
                                        .Include(r => r.Solvers);
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

            // Добавляются категории и предмет
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");
            ViewBag.Subjects = new SelectList(_db.Subjects, "Id", "Name");
            return View(request);
        }

        // POST: Request/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,CategoryId,SubjectId,Deadline,Priority,Price")] Request request, HttpPostedFileBase error)
        {
            var curId = this.HttpContext.User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                var requestBase = _db.Requests.Find(request.Id);
                if (requestBase.Author.Id == curId)
                {
                    if (error != null)
                    {
                        Document doc = new Document();
                        doc.Size = error.ContentLength;
                        // Получаем расширение
                        string ext = error.FileName.Substring(error.FileName.LastIndexOf('.'));
                        doc.Type = ext;
                        // сохраняем файл по определенному пути на сервере
                        string path = DateTime.Now.ToString(curId.GetHashCode() + "dd/MM/yyyy H:mm:ss").Replace(":", "_").Replace("/", ".") + ext;
                        error.SaveAs(Server.MapPath("~/Files/RequestFiles/" + path));
                        doc.Url = path;

                        requestBase.Document = doc;
                        _db.Documents.Add(doc);
                    }
                    
                    requestBase.Name = request.Name;
                    requestBase.Category = _db.Categories.Find(request.CategoryId);
                    requestBase.CategoryId = requestBase.Category.Id;
                    requestBase.Subject = requestBase.Subject;
                    requestBase.SubjectId = requestBase.Subject.Id;
                    requestBase.Deadline = request.Deadline;
                    requestBase.Priority = request.Priority;
                    requestBase.Description = request.Description;
                    requestBase.Price = request.Price;

                    _db.Entry(requestBase).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    return RedirectToAction("MyIndex");
                }
                else
                {
                    return Content("К сожалению, Вы не автор данной задачи и редактировать её не можете");
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
                // указываем статус Открыта у заявки
                request.Status = (int)RequestStatus.Open;
                request.IsPaid = false;

                //получаем время открытия
                DateTime current = DateTime.Now;

                //Создаем запись о жизненном цикле заявки
                Lifecycle newLifecycle = new Lifecycle() { Opened = current };
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
                    Document doc = new Document();
                    doc.Size = error.ContentLength;
                    // Получаем расширение
                    string ext = error.FileName.Substring(error.FileName.LastIndexOf('.'));
                    doc.Type = ext;
                    // сохраняем файл по определенному пути на сервере
                    string path = current.ToString(user.Id.GetHashCode()+"dd/MM/yyyy H:mm:ss").Replace(":", "_").Replace("/", ".") + ext;
                    error.SaveAs(Server.MapPath("~/Files/RequestFiles/" + path));
                    doc.Url = path;

                    request.Document = doc;
                    _db.Documents.Add(doc);
                }
                else
                    request.Document = null;

                var cat = _db.Categories.Find(request.CategoryId);
                request.Category = cat;
                var sub = _db.Subjects.Find(request.SubjectId);
                request.Subject = sub;

                request.Checked = false;
                request.CanDownload = false;

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
                    return Content(e.Message);
                }

                return RedirectToAction("Index");
            }
            return View(request);
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
                                        .Include(r => r.Category)  // добавляем категории
                                        .Include(r => r.Lifecycle)  // добавляем жизненный цикл заявок
                                        .Include(r => r.Author)         // добавляем данные о пользователях
                                        .Include(r=>r.Solvers)
                                        .OrderByDescending(r => r.Lifecycle.Opened); // упорядочиваем по дате по убыванию   
            }
            else
                allReqs = _db.Requests
                                        .Where(x => x.CategoryId == category)
                                        .Where(r => r.Author.Id == user.Id) //получаем заявки для текущего пользователя
                                        .Include(r => r.Category)  // добавляем категории
                                        .Include(r => r.Lifecycle)  // добавляем жизненный цикл заявок
                                        .Include(r => r.Author)         // добавляем данные о пользователях
                                        .Include(r => r.Solvers)
                                        .OrderByDescending(r => r.Lifecycle.Opened); // упорядочиваем по дате по убыванию  ;

            
            List<Category> categories = _db.Categories.ToList();

            //Добавляем в список возможность выбора всех
            categories.Insert(0, new Category { Name = "Все", Id = 0 });
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View(allReqs.ToList());
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
            Lifecycle lifecycle = _db.Lifecycles.Where(m => m.Id == id).First();

            if (lifecycle != null)
            {
                return PartialView("_Lifecycle", lifecycle);
            }
            return View("Index");
        }

        // Удаление заявки
        public void Delete(int id)
        {
            Request request = _db.Requests.Find(id);
            var curId = HttpContext.User.Identity.GetUserId();
            // получаем текущего пользователя
            ApplicationUser user = _db.Users.First(m => m.Id == curId);
            if (request != null && request.Author.Id == user.Id)
            {
                Lifecycle lifecycle = _db.Lifecycles.Find(request.LifecycleId);
                _db.Lifecycles.Remove(lifecycle);
                _db.Requests.Remove(request);
                _db.SaveChanges();
            }
            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }


        /// <summary>
        /// Редактирование заявок - назначение исполнителей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Moderator, Administrator")]
        public ActionResult Distribute(int? category)
        {
            IEnumerable<Request> requests = null;
            if (category == null || category == 0)
            {
                requests=_db.Requests.Include(r => r.Author)
                    .Include(r => r.Lifecycle)
                    .Include(r => r.Executor)
                    .Include(r => r.Document)
                    .Include(r => r.Solvers);
            }
            else
                requests = _db.Requests.Include(r => r.Author)
                    .Include(r => r.Lifecycle)
                    .Include(r => r.Executor)
                    .Include(r => r.Document)
                    .Include(r=>r.Solvers)
                    .Where(x => x.CategoryId == category);
            
            var users = _db.Users;

            var executors = users.ToList();// Думаю, пока не стоит делать только для обычных пользователей
            ViewBag.Executors = new SelectList(executors, "Id", "Name");

            List<Category> categories = _db.Categories.ToList();
            //Добавляем в список возможность выбора всех
            categories.Insert(0, new Category { Name = "Все", Id = 0 });
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View("Distribute",requests);
        }

        [HttpPost]
        [Authorize(Roles = "Moderator, Administrator")]
        public ActionResult Distribute(int? requestId, string executorId)
        {
            if (requestId == null && executorId.IsEmpty())// == null)
            {
                return RedirectToAction("Distribute");
            }
            Request req = _db.Requests.Find(requestId);
            ApplicationUser ex = _db.Users.Find(executorId);
            if (req == null && ex == null)
            {
                return RedirectToAction("Distribute");
            }
            req.ExecutorId = executorId;

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
                var requests = _db.Requests.Include(r => r.Author)
                                    .Include(r => r.Lifecycle)
                                    .Include(r => r.Executor)
                                    .Include(r=>r.Document)
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
                return Content("К сожалению, Вы не исполнитель данной заявки и не можете изменить её статус");
            }

            if (req != null)
            {
                req.Status = status;
                Lifecycle lifecycle = _db.Lifecycles.Find(req.LifecycleId);
                if (status == (int)RequestStatus.Open)
                {
                    lifecycle.Opened = DateTime.Now;
                }
                if (status == (int)RequestStatus.Distributed)
                {
                    lifecycle.Distributed = DateTime.Now;
                }
                if (status == (int)RequestStatus.Proccesing)
                {
                    lifecycle.Proccesing = DateTime.Now;
                }
                else if (status == (int)RequestStatus.Closed)
                {
                    lifecycle.Closed = DateTime.Now;
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
        public ActionResult GetCountOfAllRequests()
        {
            string count = _db.Requests.Count().ToString();
            ViewBag.Message = count;
            return PartialView("Message");
        }

        public ActionResult GetCountOfAllSolvedRequests()
        {
            string count = _db.RequestSolutions.Count().ToString();
            ViewBag.Message = count;
            return PartialView("Message");
        }

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
        

        public ActionResult AddToSolvers(string id)
        {
            var req = _db.Requests.Find(id);
            var curId = this.HttpContext.User.Identity.GetUserId();
            var thisUser = _db.Users.Find(curId);

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
            ApplicationUser ex = _db.Users.Find(executorId);
            if (req == null && ex == null)
            {
                return RedirectToAction("MyIndex");
            }
            req.ExecutorId = executorId;

            req.Status = (int)RequestStatus.Distributed;
            Lifecycle lifecycle = _db.Lifecycles.Find(req.LifecycleId);
            lifecycle.Distributed = DateTime.Now;
            _db.Entry(lifecycle).State = EntityState.Modified;

            _db.Entry(req).State = EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("MyIndex");
        }
    }



}
