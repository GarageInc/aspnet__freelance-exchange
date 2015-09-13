using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [Authorize]
    public class RequirementController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();


        [Authorize(Roles = "Administrator, Moderator")]
        public async Task<ActionResult> Index()
        {
            var requirements = _db.Requirements
                .Where(r => r.IsDeleted == false)
                .Include(r => r.Author)
                .Include(r=>r.RequirementConfirmations);

            return View(await requirements.ToListAsync());
        }

        public async Task<ActionResult> MyIndex()
        {
            var curId = this.User.Identity.GetUserId();

            var requirements = _db.Requirements
                .Where(r => r.IsDeleted == false)
                .Where(r => r.AuthorId == curId)
                .Include(r => r.Author)
                .Include(r => r.RequirementConfirmations);

            return View(await requirements.ToListAsync());
        }
        
        // GET: Requirement/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Requirement/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Description,Price")] Requirement requirement)
        {
            if (ModelState.IsValid)
            {

                var curId = this.User.Identity.GetUserId();
                var user = _db.Users.Find(curId);

                var curDate = DateTime.Now;
                requirement.IsBlocked = false;
                requirement.BlockReason = "";
                requirement.BlockForDate= curDate;
                requirement.DateOfBlocking = curDate;

                requirement.CreateDateTime= curDate;
                requirement.Author = user;
                requirement.AuthorId = user.Id;
                requirement.Checked = false;
                requirement.CanDownload = false;
                requirement.IsDeleted = false;
                requirement.Closed = false;

                _db.Requirements.Add(requirement);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            
            return View(requirement);
        }

        // GET: Requirement/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Requirement requirement = await _db.Requirements.FindAsync(id);
            if (requirement == null)
            {
                return HttpNotFound();
            }
            if (ValidateRequirement(requirement)==false)
            {
                return Content("Действие невозможно, т.к. заявка закрыта");
            }

            return View(requirement);
        }

        // POST: Requirement/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Description,Price")] Requirement requirement)
        {
            if (ValidateRequirement(requirement) == false)
            {
                return Content("Действие невозможно, т.к. заявка закрыта");
            }

            if (ModelState.IsValid)
            {
                var baseRequirement = _db.Requirements.Find(requirement.Id);

                baseRequirement.Description = requirement.Description;
                baseRequirement.Price = requirement.Price;

                _db.Entry(baseRequirement).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("MyIndex");
            }

            ViewBag.AuthorId = new SelectList(_db.Users, "Id", "Name", requirement.AuthorId);
            return View(requirement);
        }

        /// <summary>
        /// Удаление заявки
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var requirement = _db.Requirements
                .Where(r => r.IsDeleted == false)
                .Where(x => x.Id == id)
                .Include(e => e.Author)
                .First();

            if (requirement != null)
            {
                return PartialView("_Delete", requirement);
            }
            return View("Index");
        }

        /// Удаление заявки
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public void DeleteConfirmed(int id)
        {
            Requirement requirement = _db.Requirements.Find(id);

            if (requirement != null)
            {
                if (ValidateRequirement(requirement) == false)
                {
                    return;
                }

                requirement.IsDeleted = true;
                _db.Entry(requirement).State = EntityState.Modified;

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
        /// Проверка вводимой стоимости
        /// </summary>
        /// <param name="Price"></param>
        /// <returns></returns>
        public JsonResult CheckPrice(decimal Price)
        {
            var curId = this.User.Identity.GetUserId();
            var user = _db.Users.Find(curId);

            bool result = !(user.Balance < Price);
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Покажем детали блокирования
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult RequirementBlockedDetails(int id)
        {
            var requirement = _db.Requirements.Find(id);

            if (ValidateRequirement(requirement) == false)
            {
                return Content("Действие невозможно, т.к. заявка закрыта");
            }

            return PartialView("_RequirementBlockedDetails",requirement);
        }

        /// <summary>
        /// Оплаченность изменяется
        /// </summary>
        /// <param name="requirementId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult RequirementChangeStatus(int? requirementId, int status)
        {
            var curId = HttpContext.User.Identity.GetUserId();
            ApplicationUser userAuth = _db.Users.First(m => m.Id == curId);

            if (userAuth == null)
            {
                return RedirectToAction("LogOff", "Account");
            }

            Requirement requirement = _db.Requirements.Find(requirementId);

            if (ValidateRequirement(requirement) == false)
            {
                return Content("Действие невозможно, т.к. заявка закрыта");
            }

            if (requirement != null)
            {
                // Берем того, кто отправил заявку
                ApplicationUser author = _db.Users.First(m => m.Id == requirement.AuthorId);
                {
                    switch (status)
                    {
                        case 0:
                            {
                                // Т.о. нельзя "опустить" в минус
                                if (requirement.Checked == true)
                                {
                                    // Оплата - не проверена
                                    requirement.Checked = false;
                                }

                                break;
                            }

                        case 1:
                            {
                                // Данное действие в любом случае будет первым
                                if (requirement.Checked == false)
                                {
                                    requirement.Checked = true;
                                }

                                break;
                            }
                    }
                }
            }

            _db.Entry(requirement).State = EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Получение всех подтверждений об оплате данной заявки
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetAllRequirementConfirmations(int id)
        {
            var reqConfs = _db.RequirementConfirmations
                .Where(x => x.IsDeleted == false)
                .Where(x => x.RequirementId == id);

            return PartialView("_GetAllRequirementConfirmations", reqConfs);
        }

        /// <summary>
        /// Разблокирование заявки
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult DownBlockRequirement(int id)
        {
            var requir = _db.Requirements.Find(id);

            if (ValidateRequirement(requir) == false)
            {
                return Content("Действие невозможно, т.к. заявка закрыта");
            }

            requir.IsBlocked = false;
            _db.Entry(requir).State = EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Блокирование заявки
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult BlockRequirement(int id)
        {
            var requir = _db.Requirements.Find(id);

            if (ValidateRequirement(requir) == false)
            {
                return Content("Действие невозможно, т.к. заявка закрыта");
            }

            return PartialView("_BlockRequirement", requir);
        }

        [Authorize(Roles = "Administrator, Moderator")]
        public void UpBlockRequirement(int requirementId, string blockReason, string blockDate)
        {
            var requir = _db.Requirements.Find(requirementId);
            if (!requir.IsBlocked)
            {
                if (ValidateRequirement(requir) == false)
                {
                    return;// Content("Действие невозможно, т.к. заявка закрыта");
                }

                requir.IsBlocked = true;
                requir.BlockReason = blockReason;
                requir.BlockForDate = DateTime.Parse(blockDate);
                requir.DateOfBlocking=DateTime.Now;;

                _db.Entry(requir).State = EntityState.Modified; ;
                _db.SaveChanges();
            }

            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        /// <summary>
        /// Закрытие и блокирование заявки
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult Close(int id)
        {
            var requir = _db.Requirements.Find(id);
            if (ValidateRequirement(requir)==false)
            {
                return Content("Действие невозможно, т.к. заявка закрыта");
            }

            requir.Closed = true;
            _db.Entry(requir).State = EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Валидация заявки
        /// </summary>
        /// <param name="requir"></param>
        /// <returns></returns>
        private bool ValidateRequirement(Requirement requir)
        {
            return !requir.Closed;
        }


    }
}
