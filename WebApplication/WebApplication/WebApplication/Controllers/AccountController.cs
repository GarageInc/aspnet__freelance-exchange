﻿namespace WebApplication.Controllers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using WebApplication.Models;
    using WebApplication.Service;
    using System.Web.Security;
    using System.Data.Entity;
    using System.Collections.Generic;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Net;
    using System.Web.WebPages;

    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        readonly ApplicationDbContext _db;
        private readonly DocumentService _docService = new DocumentService();

        public AccountController()
        {
            _db = new ApplicationDbContext();
        }

        

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _db = new ApplicationDbContext();
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public  ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _db.Users.FirstOrDefault(x => x.Email == model.Email && x.Password == model.Password);
            if(user==null)
                return View("Lockout");

            // Если нельзя заходить - то всё, заблокирован
            if (!ValidateUser(user))
            {
                string text = user.BlockReason;
                text += "\nДата окончания блокировки: ";
                text += user.BlockForDate;
                text += "\nЕсли причина блокировки неясна, то - отпишите нам, пожалуйста, по кнопке в правом нижнем углу. С уважением, администрация GarageInc";
                return Content(text);
            }

            var result =  SignInManager.PasswordSignIn(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                            if (user != null)
                            {
                                using (var transaction = _db.Database.BeginTransaction())
                                {
                                    try
                                    {
                                        user.LastVisition = DateTime.Now;
                                        _db.Entry(user).State = EntityState.Modified;

                                        _db.SaveChanges();

                                        transaction.Commit();
                                    }
                                    catch (Exception error)
                                    {
                                        transaction.Rollback();
                                        return PartialView("_Content", error.Message);
                                    }
                                }
                            }
                     
                        return RedirectToLocal(returnUrl);

                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Неудачная попытка входа.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Требовать предварительный вход пользователя с помощью имени пользователя и пароля или внешнего имени входа
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Приведенный ниже код защищает от атак методом подбора, направленных на двухфакторные коды. 
            // Если пользователь введет неправильные коды за указанное время, его учетная запись 
            // будет заблокирована на заданный период. 
            // Параметры блокирования учетных записей можно настроить в IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Неправильный код.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var curDate = DateTime.Now;
                var user = new ApplicationUser {
                    UserName = model.Email,
                    Name = model.Name,
                    Password =model.Password,
                    Email = model.Email,
                    LastVisition = curDate,
                    RegistrationDate = curDate,
                    UserInfo ="user",
                    DateOfBlocking = curDate,
                    BlockForDate = curDate,
                    IsBlocked = false,
                    BlockReason="",
                    Balance = 0
                };
               
                var result = UserManager.Create(user, model.Password);
                
                // Пока что всех делаем администраторами, потом, конечно, это стоит убрать
                if (_db.Users.Any() == false)
                    UserManager.AddToRole(user.Id, "Administrator");
                else
                {
                    UserManager.AddToRole(user.Id, "User");
                }
                if (result.Succeeded)
                {
                    SignInManager.SignIn(user, isPersistent:false, rememberBrowser:false);
                    
                    // Дополнительные сведения о том, как включить подтверждение учетной записи и сброс пароля, см. по адресу: http://go.microsoft.com/fwlink/?LinkID=320771
                    // Отправка сообщения электронной почты с этой ссылкой
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Подтверждение учетной записи", "Подтвердите вашу учетную запись, щелкнув <a href=\"" + callbackUrl + "\">здесь</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // Появление этого сообщения означает наличие ошибки; повторное отображение формы
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Не показывать, что пользователь не существует или не подтвержден
                    return View("ForgotPasswordConfirmation");
                }

                // Дополнительные сведения о том, как включить подтверждение учетной записи и сброс пароля, см. по адресу: http://go.microsoft.com/fwlink/?LinkID=320771
                // Отправка сообщения электронной почты с этой ссылкой
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Сброс пароля", "Сбросьте ваш пароль, щелкнув <a href=\"" + callbackUrl + "\">здесь</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // Появление этого сообщения означает наличие ошибки; повторное отображение формы
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Не показывать, что пользователь не существует
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Запрос перенаправления к внешнему поставщику входа
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Создание и отправка маркера
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Выполнение входа пользователя посредством данного внешнего поставщика входа, если у пользователя уже есть имя входа
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // Если у пользователя нет учетной записи, то ему предлагается создать ее
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Получение сведений о пользователе от внешнего поставщика входа
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                //var user = new ApplicationUser
                //{
                //    UserName = model.Email,
                //    Name = model.Name,
                //    Password = model.Password,
                //    Email = model.Email,
                //    LastVisition = curDate,
                //    RegistrationDate = curDate,
                //    UserInfo = "user",
                //    DateOfBlocking = curDate,
                //    BlockForDate = curDate,
                //    IsBlocked = false,
                //    BlockReason = "",
                //    Balance = 0
                //};
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Вспомогательные приложения
        // Используется для защиты от XSRF-атак при добавлении внешних имен входа
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        /// <summary>
        /// Проверка мыла на возможность существования
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public JsonResult CheckEmail(string Email)
        {
            //var result = Membership.FindUsersByEmail(Email).Count == 0;

            var result = !_db.Users.Any(x => x.Email==Email);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Создание пользователей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            SelectList roles = new SelectList(_db.Roles, "Id", "Name");
            ViewBag.Roles = roles;
            return View();
        }

        /// <summary>
        /// Создание пользователей
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterViewModel model)
        {
            var curDate = DateTime.Now;
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Name = model.Name,
                Password = model.Password,
                Email = model.Email,
                LastVisition = curDate,
                RegistrationDate = curDate,
                UserInfo = "user",
                BlockForDate = curDate,
                DateOfBlocking = curDate,
                IsBlocked = false,
                BlockReason = "",
                Balance=0
            };

            var role = _db.Roles.First(x => x.Id == model.RoleId);
            // создадим пользователя
            var result = await UserManager.CreateAsync(user, model.Password);
            // и добавим его к роли
            UserManager.AddToRole(user.Id, role.Name);
            
            var users = _db.Users.ToList();
            return View("Index", users);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var users = _db.Users.ToList();
            return View(users);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(string id)
        {
            ApplicationUser user = _db.Users.Find(id);
            SelectList roles = new SelectList(_db.Roles, "Id", "Name");
            ViewBag.Roles = roles;

            EditViewModel regWm = new EditViewModel();
            regWm.Name = user.Name;
            regWm.Password = user.Password;
            regWm.Email = user.Email;
            regWm.Id = user.Id;

            return View(regWm);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(EditViewModel user)
        {
            if (ModelState.IsValid)
            {
                var userCurrent = _db.Users.Find(user.Id);

                string pass = user.Password;

                userCurrent.Name = user.Name;
                userCurrent.UserName = user.Email;
                userCurrent.Email = user.Email;
                userCurrent.Password = user.Password;
                userCurrent.PasswordHash = UserManager.PasswordHasher.HashPassword(pass);

                // Теперь разберемся с изменением роли 
#warning TODO Крайне кривая конструкция
                var allRoles = new List<IdentityRole>();
                var userRoles = userCurrent.Roles;

                // Соберем все роли
                foreach(var ur in userRoles)
                {
                    allRoles.Add(_db.Roles.First(x => x.Id == ur.RoleId));
                }
                // Удалим роли
                foreach(var r in allRoles)
                {
                    UserManager.RemoveFromRole(user.Id, r.Name);
                }
                // Добавим новую роль
                var newRole = _db.Roles.First(x => x.Id == user.RoleId).Name;
                UserManager.AddToRole(user.Id, newRole);

                // Сохраним изменения
                _db.Entry(userCurrent).State = EntityState.Modified;

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        _db.SaveChanges();

                        transaction.Commit();
                    }
                    catch (Exception error)
                    {
                        transaction.Rollback();
                        return PartialView("_Content", error.Message);
                    }
                }

                return RedirectToAction("Index");
            }
            
            SelectList roles = new SelectList(_db.Roles, "Id", "Name");
            ViewBag.Roles = roles;

            return View(user);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(string id)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    ApplicationUser user = _db.Users.Find(id);
                    _db.Users.Remove(user);

                    _db.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception error)
                {
                    transaction.Rollback();
                    return PartialView("_Content", error.Message);
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Details(string id)
        {
            if (id.IsEmpty())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = _db.Users.Where(x=>x.Id==id).Include(x=>x.Props).Include(x=>x.Contacts).FirstOrDefault();
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        
        // ПОДРОБНОСТИ О ПОЛЬЗОВАТЕЛЕ
        public ActionResult UserDetails(string userId)
        {
            var user = _db.Users.Find(userId);

            using (_db)
            {
                var comms = _db.RecallMessages
                        .Where(r => r.User.Id == userId)
                        .Where(x => !x.IsDeleted)
                        .Include(x => x.Author);

                RecallMessageList model = new RecallMessageList()
                {
                    RecallMessages = comms.ToArray()
                };
                
                ViewData["User"] = user;

                return View(model);
            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public void Add(string userId, int? parentId, string Text)
        {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var curId = this.HttpContext.User.Identity.GetUserId();
                        var author = _db.Users.Find(curId);
                        var user = _db.Users.Find(userId);

                        var newRecallMessages = new RecallMessage()
                        {
                            ParentId = parentId,
                            Text = Text,
                            Author = author,
                            AuthorId = author.Id,
                            CreateDateTime = DateTime.Now,
                            Karma = 0,
                            IsDeleted = false,
                            AboutSite = false,
                            User = user,
                            UserId = user.Id
                        };

                        _db.RecallMessages.Add(newRecallMessages);
                        _db.SaveChanges();

                        transaction.Commit();
                    }
                    catch (Exception error)
                    {
                        transaction.Rollback();
                        throw new Exception(error.Message);
                    }
                }
                
            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public void Move(int nodeId, int? newParentId)
        {
            if (nodeId == newParentId)
            {
                Response.Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            
                if (newParentId.HasValue && ContainsChilds(_db, nodeId, newParentId.Value))
                {
                    Response.Redirect(Request.UrlReferrer.AbsoluteUri);
                }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var node = _db.RecallMessages.Where(x => x.Id == nodeId).Single();
                    node.ParentId = newParentId;
                    _db.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception error)
                {
                    transaction.Rollback();
                    throw new Exception(error.Message);
                }
            }           
            
            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        private bool ContainsChilds(ApplicationDbContext db, int parentId, int id)
        {
            bool result = false;
            var inner = db.RecallMessages.Where(x => x.ParentId == parentId && !x.IsDeleted).ToArray();
            foreach (var node in inner)
            {
                if (node.Id == id && node.ParentId == parentId)
                {
                    return true;
                }
                result = ContainsChilds(db, node.Id, id);
            }
            return result;
        }

        public void DeleteRecall(string id)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    DeleteNodes(_db, int.Parse(id));
                    _db.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception error)
                {
                    transaction.Rollback();
                    throw new Exception(error.Message);
                }
            }
            
            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        private void DeleteNodes(ApplicationDbContext db, int id)
        {
            var inner = db.RecallMessages.Where(x => x.ParentId == id && !x.IsDeleted).ToArray();
            foreach (var node in inner)
            {
                node.IsDeleted = true;
                DeleteNodes(db, node.Id);
            }
            var deleted = db.RecallMessages.Where(x => x.Id == id && !x.IsDeleted).Single();
            deleted.IsDeleted = true;
        }

        public void UpRecallMessage(int id)
        {
            var curId = this.User.Identity.GetUserId();
            var user = _db.Users.Find(curId);

            var com = _db.RecallMessages.Find(id);

            if (!user.UpRecalls.Where(x => x.Id == id).Any())
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {

                        com.Karma++;

                        user.UpRecalls.Add(com);
                        user.DownRecalls.Remove(com);

                        _db.Entry(user).State = EntityState.Modified; ;
                        _db.Entry(com).State = EntityState.Modified;
                        _db.SaveChanges();

                        transaction.Commit();
                    }
                    catch (Exception error)
                    {
                        transaction.Rollback();
                        throw new Exception(error.Message);
                    }
                }
            }

            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        public void DownRecallMessage(int id)
        {
            var curId = this.User.Identity.GetUserId();
            var user = _db.Users.Find(curId);

            var com = _db.RecallMessages.Find(id);

            if (!user.DownRecalls.Any(x => x.Id == id))
            {
                com.Karma--;

                user.DownRecalls.Add(com);
                user.UpRecalls.Remove(com);

                _db.Entry(user).State = EntityState.Modified; ;
                _db.Entry(com).State = EntityState.Modified;
                _db.SaveChanges();
            }

            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }


        [AllowAnonymous]
        public ActionResult UserDetailsModal(string id)
        {
            ApplicationUser executor = _db.Users.First(m => m.Id == id);

            if (executor != null)
            {
                return PartialView("_UserDetailsModal", executor);
            }
            return View("Index");
        }


        /// <summary>
        /// Разблокирование пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Moderator, Administrator")]
        public void DownBlockOfUser(string userId)
        {
            var user = _db.Users.Find(userId);
            if (user.IsBlocked)
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        user.IsBlocked = false;

                        _db.Entry(user).State = EntityState.Modified; ;
                        _db.SaveChanges();

                        transaction.Commit();
                    }
                    catch (Exception error)
                    {
                        transaction.Rollback();
                        throw new Exception(error.Message);
                    }
                }
            }

            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }
        /// <summary>
        /// Блокирование пользователя
        /// </summary>
        /// <param name="userId"></param>
        [Authorize(Roles = "Moderator, Administrator")]
        public void UpBlockOfUser(string userId, string blockReason, DateTime blockDate)
        {
            var user = _db.Users.Find(userId);
            if (!user.IsBlocked)
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        user.IsBlocked = true;
                        user.BlockReason = blockReason;
                        user.BlockForDate = blockDate;
                        user.DateOfBlocking = DateTime.Now; ;

                        _db.Entry(user).State = EntityState.Modified; ;
                        _db.SaveChanges();

                        transaction.Commit();
                    }
                    catch (Exception error)
                    {
                        transaction.Rollback();
                        throw new Exception(error.Message);
                    }
                }
            }

            Response.Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        /// <summary>
        /// Валидация пользователя при входе в систему
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ValidateUser(ApplicationUser user)
        {
            var currentDate = DateTime.Now;

            bool validated = true;
            if(user.IsBlocked)
            {
                validated = !(currentDate < user.BlockForDate);
            }
            
            return validated;
        }
    }
}

//IdentityResult ir;
//var rm = new RoleManager<IdentityRole>
//    (new RoleStore<IdentityRole>(context));
//ir = rm.Create(new IdentityRole("Administrator"));