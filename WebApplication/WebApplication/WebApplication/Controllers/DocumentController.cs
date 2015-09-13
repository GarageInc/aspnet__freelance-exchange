﻿using System;
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
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Documents
        public async Task<ActionResult> Index()
        {
            return View(await _db.Documents.ToListAsync());
        }

        // GET: Documents/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = await _db.Documents.FindAsync(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // GET: Documents/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Documents/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Url,Size,Type")] Document document)
        {
            if (ModelState.IsValid)
            {
                _db.Documents.Add(document);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(document);
        }

        // GET: Documents/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = await _db.Documents.FindAsync(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // POST: Documents/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Url,Size,Type")] Document document)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(document).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(document);
        }

        // GET: Documents/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = await _db.Documents.FindAsync(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Document document = await _db.Documents.FindAsync(id);
            _db.Documents.Remove(document);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }


        public FileResult DownloadPaymentFiles(int id)
        {
            var reqDoc = _db.Documents.Find(id);//.Document;

            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Files/PaymentFiles/" + reqDoc.Url));
            string fileName = reqDoc.Id + reqDoc.Type;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public FileResult DownloadRequestSolutionFiles(int id)
        {
            var reqDoc = _db.Documents.Find(id);//.Document;

            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Files/RequestSolutionFiles/" + reqDoc.Url));
            string fileName = reqDoc.Id + reqDoc.Type;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        
        public FileResult DownloadRequestFile(int id)
        {
            var reqDoc = _db.Documents.Find(id);//.Document;

            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Files/RequestFiles/" + reqDoc.Url));
            string fileName = reqDoc.Id + reqDoc.Type;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public FileResult DownloadErrorMessageFile(int id)
        {
            var reqDoc = _db.Documents.Find(id);//.Document;

            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Files/ErrorMessageFiles/" + reqDoc.Url));
            string fileName = reqDoc.Id + reqDoc.Type;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        /// <summary>
        /// Загрузка решения
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileResult DownloadSolution(int id)
        {
            var reqDoc = _db.Documents.First(x => x.Id == id);//...Find(id).Documents.Find(id);//.Document;

            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Files/RequestSolutionFiles/" + reqDoc.Url));
            string fileName = reqDoc.Id + reqDoc.Type;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        /// <summary>
        /// Загрузка подтверждения об оплате
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileResult DownloadRequirementConfirmation(int id)
        {
            var reqDoc = _db.Documents.First(x=>x.Id==id);//...Find(id).Documents.Find(id);//.Document;

            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Files/RequirementConfirmationFiles/" + reqDoc.Url));
            string fileName = reqDoc.Id + reqDoc.Type;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}
