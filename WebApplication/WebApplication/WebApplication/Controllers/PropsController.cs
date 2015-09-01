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
    public class PropsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        // POST: Props/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,PropsCategoryId,Number")] Props props)
        {
            if (ModelState.IsValid)
            {

                var curId = this.User.Identity.GetUserId();
                var user = db.Users.Find(curId);

                var propsCat = db.PropsCategories.Find(props.PropsCategoryId);

                props.PropsCategory = propsCat;
                
                props.Author = user;
                props.AuthorId = user.Id;

                user.Props.Add(props);
                db.Entry(user).State=EntityState.Modified;
                db.Props.Add(props);

                await db.SaveChangesAsync();

                return RedirectToAction("Index", "Manage");
            }
            
            return RedirectToAction("Index","Manage");
        }

        // POST: Props/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            Props props = await db.Props.FindAsync(id);
            db.Props.Remove(props);
            await db.SaveChangesAsync();
            return RedirectToAction("Index","Manage");
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
