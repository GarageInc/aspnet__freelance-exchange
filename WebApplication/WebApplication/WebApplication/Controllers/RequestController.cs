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

    public class RequestController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // поиск задач по категориям
        [HttpPost]
        public ActionResult Index(int? category)
        {
            IEnumerable<Request> allReqs = null;
            if (category == null || category == 0)
            {
                allReqs = db.Requests
                    //.Where(x => x.Checked)
                    ;
            }
            else
                allReqs = db.Requests.Where(x => x.CategoryId == category)
                    //.Where(x=>x.Checked)
                    ;


            List<Category> categories = db.Categories.ToList();
            //Добавляем в список возможность выбора всех
            categories.Insert(0, new Category { Name = "Все", Id = 0 });
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View(allReqs.ToList());
        }

        // GET: Request/Edit/5
        //[Authorize]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Request request = await db.Requests.FindAsync(id);
            if (request == null)
            {
                return HttpNotFound();
            }
            return View(request);
        }

        // POST: Request/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,Comment,Deadline,Status,Priority")] Request request)
        {
            if (ModelState.IsValid)
            {
                db.Entry(request).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(request);
        }
        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        //[Authorize]
        public ActionResult Create()
        {
            var curId = HttpContext.User.Identity.GetUserId();
            // получаем текущего пользователя
            ApplicationUser user = db.Users.Where(m => m.Id == curId).FirstOrDefault();
            if (user != null)
            {
                // Добавляются категории
                ViewBag.Categories = new SelectList(db.Categories, "Id", "Name");
                ViewBag.Subjects = new SelectList(db.Subjects, "Id", "Name");
                var model = new Request();
                model.Deadline=DateTime.Now;

                return View(model);
            }
            return RedirectToAction("LogOff", "Account");
        }

        // Создание новой заявки
        [HttpPost]
        //[Authorize]
        public ActionResult Create(Request request, HttpPostedFileBase error)
        {
            var curId = HttpContext.User.Identity.GetUserId();
            // получаем текущего пользователя
            ApplicationUser user = db.Users.FirstOrDefault(m => m.Id == curId);
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
                db.Lifecycles.Add(newLifecycle);

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
                    db.Documents.Add(doc);
                }
                else
                    request.Document = null;

                var cat = db.Categories.Find(request.CategoryId);
                request.Category = cat;
                var sub = db.Subjects.Find(request.SubjectId);
                request.Subject = sub;

                request.Checked = false;
                request.Status = (int)RequestStatus.Open;

                //Добавляем заявку с возможно приложенными документами
                db.Requests.Add(request);
                user.Requests.Add(request);
                db.Entry(user).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
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
        //[Authorize]
        public ActionResult MyIndex(int? category)
        {
            var currentId = HttpContext.User.Identity.GetUserId();
            // получаем текущего пользователя
            ApplicationUser user = db.Users.Where(m => m.Id == currentId).FirstOrDefault();
            IEnumerable<Request> allReqs = null;
            if (category == null || category == 0)
            {

                allReqs = db.Requests.Where(r => r.Author.Id == user.Id)
                    //.Where(x => x.Checked) //получаем заявки для текущего пользователя
                                        .Include(r => r.Category)  // добавляем категории
                                        .Include(r => r.Lifecycle)  // добавляем жизненный цикл заявок
                                        .Include(r => r.Author)         // добавляем данные о пользователях
                                        .Include(r=>r.Solvers)
                                        .OrderByDescending(r => r.Lifecycle.Opened); // упорядочиваем по дате по убыванию   
            }
            else
                allReqs = db.Requests
                    //.Where(x => x.Checked)
                    .Where(x => x.CategoryId == category)
                    .Where(r => r.Author.Id == user.Id) //получаем заявки для текущего пользователя
                                        .Include(r => r.Category)  // добавляем категории
                                        .Include(r => r.Lifecycle)  // добавляем жизненный цикл заявок
                                        .Include(r => r.Author)         // добавляем данные о пользователях
                                        .Include(r => r.Solvers)
                                        .OrderByDescending(r => r.Lifecycle.Opened); // упорядочиваем по дате по убыванию  ;



            List<Category> categories = db.Categories.ToList();
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
        public ActionResult MyDetails(int id)
        {
            Request request = db.Requests.Find(id);

            if (request != null)
            {
                //получаем категорию
                request.Category = db.Categories.First(m => m.Id == request.CategoryId);
                return PartialView("_Details", request);
            }
            return View("MyIndex");
        }

        //[Authorize]
        public ActionResult MyExecutor(string id)
        {
            ApplicationUser executor = db.Users.First(m => m.Id == id);

            if (executor != null)
            {
                return PartialView("_Executor", executor);
            }
            return View("MyIndex");
        }

        //[Authorize]
        public ActionResult MyLifecycle(int id)
        {
            Lifecycle lifecycle = db.Lifecycles.First(m => m.Id == id);

            if (lifecycle != null)
            {
                return PartialView("_Lifecycle", lifecycle);
            }
            return View("MyIndex");
        }

        // Удаление заявки
        //[Authorize]
        public ActionResult MyDelete(int id)
        {
            Request request = db.Requests.Find(id);
            // получаем текущего пользователя
            var curId = HttpContext.User.Identity.GetUserId();
            ApplicationUser user = db.Users.First(m => m.Id == curId);
            if (request != null && request.Author.Id == user.Id)
            {
                Lifecycle lifecycle = db.Lifecycles.Find(request.LifecycleId);
                db.Lifecycles.Remove(lifecycle);
                db.SaveChanges();
            }
            return RedirectToAction("MyIndex");
        }

        

        /// <summary>
        /// Скачивание файла
        /// </summary>
        /// <returns></returns>
        public FileResult Download(int id)
        {
            var req = db.Requests.Find(id);
            var reqDoc = req.Document;
            
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Files/RequestFiles/" + reqDoc.Url));
                string fileName = req.Id + reqDoc.Type;
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }



        /// <summary>
        /// Получение заявок текущего пользователя
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize]
        public ActionResult Index()
        {
            var requests = db.Requests
                //.Where(x => x.Checked)
                .Include(r => r.Category)  // добавляем категории
                                    .Include(r => r.Lifecycle)  // добавляем жизненный цикл заявок
                                    .Include(r => r.Author)         // добавляем данные о пользователях
                                    .OrderByDescending(r => r.Lifecycle.Opened); // упорядочиваем по дате по убыванию  
             
            List<Category> categories = db.Categories.ToList();
            //Добавляем в список возможность выбора всех
            categories.Insert(0, new Category { Name = "Все", Id = 0 });
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View(requests.ToList());
        }


        /// <summary>
        /// Просмотр подробных сведений о заявке
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Details(int id)
        {
            Request request = db.Requests.Find(id);

            if (request != null)
            {
                //получаем категорию
                request.Category = db.Categories.Where(m => m.Id == request.CategoryId).First();
                return PartialView("_Details", request);
            }
            return View("Index");
        }

        //[Authorize]
        public ActionResult Author(string id)
        {
            ApplicationUser executor = db.Users.Where(m => m.Id == id).First();

            if (executor != null)
            {
                return PartialView("_Executor", executor);
            }
            return View("Index");
        }

        //[Authorize]
        public ActionResult Executor(string id)
        {
            ApplicationUser executor = db.Users.Where(m => m.Id == id).First();

            if (executor != null)
            {
                return PartialView("_Executor", executor);
            }
            return View("Index");
        }

        //[Authorize]
        public ActionResult Lifecycle(int id)
        {
            Lifecycle lifecycle = db.Lifecycles.Where(m => m.Id == id).First();

            if (lifecycle != null)
            {
                return PartialView("_Lifecycle", lifecycle);
            }
            return View("Index");
        }

        // Удаление заявки
        //[Authorize]
        public ActionResult Delete(int id)
        {
            Request request = db.Requests.Find(id);
            var curId = HttpContext.User.Identity.GetUserId();
            // получаем текущего пользователя
            ApplicationUser user = db.Users.First(m => m.Id == curId);
            if (request != null && request.Author.Id == user.Id)
            {
                Lifecycle lifecycle = db.Lifecycles.Find(request.LifecycleId);
                db.Lifecycles.Remove(lifecycle);
                db.Requests.Remove(request);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Редактирование заявок - назначение исполнителей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Roles = "Moderator")]
        //[Authorize(Roles = "Administrator")]
        //[Authorize]
        public ActionResult Distribute(int? category)
        {
            IEnumerable<Request> requests = null;
            if (category == null || category == 0)
            {
                requests=db.Requests.Include(r => r.Author)
                    .Include(r => r.Lifecycle)
                    .Include(r => r.Executor)
                    .Include(r => r.Document)
                    .Include(r => r.Solvers);
            }
            else
                requests = db.Requests.Include(r => r.Author)
                    .Include(r => r.Lifecycle)
                    .Include(r => r.Executor)
                    .Include(r => r.Document)
                    .Include(r=>r.Solvers)
                    .Where(x => x.CategoryId == category);
            
            var users = db.Users;

            var executors = users.ToList();// Думаю, пока не стоит делать только для обычных пользователей
            ViewBag.Executors = new SelectList(executors, "Id", "Name");

            List<Category> categories = db.Categories.ToList();
            //Добавляем в список возможность выбора всех
            categories.Insert(0, new Category { Name = "Все", Id = 0 });
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View("Distribute",requests);
        }

        [HttpPost]
        //[Authorize(Roles = "Модератор")]
        //[Authorize(Roles = "Administrator")]
        //[Authorize]
        public ActionResult Distribute(int? requestId, string executorId)
        {
            if (requestId == null && executorId.IsEmpty())// == null)
            {
                return RedirectToAction("Distribute");
            }
            Request req = db.Requests.Find(requestId);
            ApplicationUser ex = db.Users.Find(executorId);
            if (req == null && ex == null)
            {
                return RedirectToAction("Distribute");
            }
            req.ExecutorId = executorId;

            req.Status = (int)RequestStatus.Distributed;
            Lifecycle lifecycle = db.Lifecycles.Find(req.LifecycleId);
            lifecycle.Distributed = DateTime.Now;
            db.Entry(lifecycle).State = EntityState.Modified;

            db.Entry(req).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Distribute");
        }


        //Заявки для изменения статуса исполнителем
        [HttpGet]
        //[Authorize]
        public ActionResult ChangeStatus()
        {
            // получаем текущего пользователя
            var curId = HttpContext.User.Identity.GetUserId();
            ApplicationUser user = db.Users.Where(m => m.Id == curId).FirstOrDefault();
            if (user != null)
            {
                var requests = db.Requests.Include(r => r.Author)
                                    .Include(r => r.Lifecycle)
                                    .Include(r => r.Executor)
                                    .Include(r=>r.Document)
                                    .Where(r => r.ExecutorId == user.Id);
                                    //.Where(r=>r.Checked==false)
                                    //.Where(r => r.Status != (int)RequestStatus.Closed);
                return View(requests);
            }
            return RedirectToAction("LogOff", "Account");
        }

        [HttpPost]
        //[Authorize]
        public ActionResult ChangeStatus(int requestId, int status)
        {
            var curId = HttpContext.User.Identity.GetUserId();
            ApplicationUser user = db.Users.First(m => m.Id == curId);

            if (user == null)
            {
                return RedirectToAction("LogOff", "Account");
            }

            Request req = db.Requests.Find(requestId);
            if (req != null)
            {
                req.Status = status;
                Lifecycle lifecycle = db.Lifecycles.Find(req.LifecycleId);
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
                db.Entry(lifecycle).State = EntityState.Modified;
                db.Entry(req).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("ChangeStatus");
        }

        /// <summary>
        /// Статистика
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCountOfAllRequests()
        {
            string count = db.Requests.Count().ToString();
            ViewBag.Message = count;
            return PartialView("Message");
        }

        public ActionResult GetCountOfAllSolvedRequests()
        {
            string count = db.RequestSolutions.Count().ToString();
            ViewBag.Message = count;
            return PartialView("Message");
        }

        public ActionResult GetCountOfAllUsers()
        {
            string count = db.Users.Count().ToString();
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
        //[Authorize]
        public ActionResult DistributeChangeStatus(int requestId, int status)
        {
            var curId = HttpContext.User.Identity.GetUserId();
            ApplicationUser user = db.Users.First(m => m.Id == curId);

            if (user == null)
            {
                return RedirectToAction("LogOff", "Account");
            }

            Request req = db.Requests.Find(requestId);
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
                
                db.Entry(req).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Distribute");
        }

        //[Authorize]
        public ActionResult AddToSolvers(string id)
        {
            var req = db.Requests.Find(id);
            var curId = this.User.Identity.GetUserId();
            var thisUser = db.Users.Find(curId);

            if (req.Solvers.Count(x => x.Id == curId) == 0)
            {
                req.Solvers.Add(thisUser);
            }

            db.Entry(req).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        //[Authorize(Roles = "Модератор")]
        //[Authorize(Roles = "Administrator")]
        //[Authorize]
        public ActionResult MySelfDistribute(int? requestId, string executorId)
        {
            if (requestId == null && executorId.IsEmpty())// == null)
            {
                return RedirectToAction("MyIndex");
            }
            Request req = db.Requests.Find(requestId);
            ApplicationUser ex = db.Users.Find(executorId);
            if (req == null && ex == null)
            {
                return RedirectToAction("MyIndex");
            }
            req.ExecutorId = executorId;

            req.Status = (int)RequestStatus.Distributed;
            Lifecycle lifecycle = db.Lifecycles.Find(req.LifecycleId);
            lifecycle.Distributed = DateTime.Now;
            db.Entry(lifecycle).State = EntityState.Modified;

            db.Entry(req).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("MyIndex");
        }
    }



}
