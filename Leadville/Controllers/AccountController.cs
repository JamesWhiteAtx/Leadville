using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Text;
using System.Text.RegularExpressions;
using CST.Security;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;
using CST.ActionFilters;
using CST.Localization;

namespace CST.Prdn.Controllers
{
    public class AccountController : CstControllerBase
    {
        //
        // GET: /Account/LogOff
        [AllowAnonymous]
        [Authorize]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            ClearUserSettings();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/LogOn
        [AllowAnonymous]
        public ActionResult LogOn(FormCollection col)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Unauthorized");
            }
            return View();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        [AllowAnonymous]
        public ActionResult LogOn(LogOnModel model, FormCollection col)
        {
            if (ModelState.IsValid)
            {
                string returnUrl = model.ReturnUrl;

                if (ValidateUserPassword(model.Login, model.Password))
                {
                    User user = GetUserByLogin(model.Login);

                    SetSaveCurrentCulture(user.Culture);
                    if (user.IfNotNull(u => u.AlterPassword) == true)
                    {
                        return RedirectToAction("LogonChangePassword", model);
                    }
                    else
                    {
                        AuthorizeUser(user, model.RememberMe);
                        return RedirectReturn(returnUrl);
                    }
                }
                else
                {
                    ModelState.AddModelError("", 
                        SystemExtensions.Sentence(model.GetDisplayName(m => m.Login), LocalStr.or, model.GetDisplayName(m => m.Password), LocalStr.verbIsNot, LocalStr.valid)
                    );
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        protected bool ValidateUserPassword(string login, string password)
        {
            return Membership.ValidateUser(login, password);
        }

        protected User GetUserByLogin(string login) 
        {
            login = login.Trim();
            return (from u in PrdnDBContext.Users
                     where u.Login == login
                     select u).FirstOrDefault();
        }

        protected bool AuthorizeUser(string login, bool remember)
        {
            User user = GetUserByLogin(login);
            if (user != null)
            {
                AuthorizeUser(user, remember);
                return true;
            }
            return false;
        }
        
        protected void AuthorizeUser(User user, bool remember)
        {
            // //this stuff is unpacked in CstBaseController OnAuthorization
            //string userData = CustomPrincipalSerializeModel.SerializeFromUser(user);
            //FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
            //         1,
            //         user.Login,
            //         DateTime.Now,
            //         DateTime.Now.AddMinutes(15),
            //         false,
            //         userData);
            //string encTicket = FormsAuthentication.Encrypt(authTicket);
            //HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            //Response.Cookies.Add(faCookie);
            FormsAuthentication.SetAuthCookie(user.GetLoginUpper(), remember);
        }

        protected ActionResult RedirectReturn(string url)
        {
            if ((url != null) 
                && (url.Length > 1) 
                && url.StartsWith("/") 
                && !url.StartsWith("//") 
                && !url.StartsWith("/\\")) 
            {
                return RedirectIfLocal(url, () => RedirectToAction("Index", "Home"));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult LogonChangePassword(LogOnModel logonModel)
        {
            LogonChangePasswordModel changeModel = new LogonChangePasswordModel
            {
                Login = logonModel.Login,
                OldPassword = logonModel.Password,
                NewPassword = null,
                RememberMe = logonModel.RememberMe,
                ReturnUrl = logonModel.ReturnUrl
            };
            
            ModelState.Clear();
            return View(changeModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult LogonChangePassword(LogonChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (ValidateUserPassword(model.Login, model.OldPassword))
                {
                    if (ChangeUserPassword(model.Login, model.OldPassword, model.NewPassword,
                        model.FullPropertyName(m => m.NewPassword), model.FullPropertyName(m => m.OldPassword)))
                    {
                        if (AuthorizeUser(model.Login, model.RememberMe))
                        {
                            return RedirectReturn(model.ReturnUrl);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }
            return View(model);
        }
        
        protected bool ChangeUserPassword(string login, string oldPassword, string newPassword, string newPropName, string oldProdName)
        {
            if (oldPassword == newPassword)
            {
                ModelState.AddModelError(newPropName, "Matches existing password.");
                return false;
            }
            
            if (!ValidPasswordPolicy(newPassword, newPropName))
            { 
                return false;
            }

            bool passwordChanged = false;
            try
            {
                //MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                //passwordChanged = currentUser.ChangePassword(oldPassword, newPassword);
                User user = GetUserByLogin(login);
                if (user != null)
                {
                    if (user.PlainPasswordMatch(oldPassword))
                    {
                        user.PlainPassword = newPassword;
                        user.AlterPassword = false;

                        PrdnDBContext.SaveChanges();
                        passwordChanged = true;
                    } 
                    else
                    {
                        ModelState.AddModelError(oldProdName, "Invalid password.");
                    }
                } 
                else
                {
                    ModelState.AddModelError("", "Change password failed, Login '"+login+"' is invalid.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Change password failed.");
            }
            return passwordChanged;
        }

        ///
        // GET: /Account/ChangePassword
        // [Authorize]
        [HttpGet]
        public ActionResult ChangePassword()
        {
            ChangePasswordModel model = new ChangePasswordModel { Login = GetCurrentUserLogin() };

            return View(model);
        }

        ///
        // POST: /Account/ChangePassword
        //[Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if ( ChangeUserPassword(model.Login, model.OldPassword, model.NewPassword,
                    model.FullPropertyName(m => m.NewPassword), model.FullPropertyName(m => m.OldPassword)))
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess
        [HttpGet]
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult _PasswordPolicy()
        {
            return Json(CST.Security.PasswordSettings.GetPasswordSettings(), JsonRequestBehavior.AllowGet);
        }

        [CstAuthorize(Groups = "ADMIN/MAINT")]
        public ActionResult _GeneratePassword()
        {
            PasswordSettings passwordSettings = CST.Security.PasswordSettings.GetPasswordSettings();
            string pw = passwordSettings.GeneratePassword();
            return Json(pw, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult PasswordSettings()
        {
            PasswordSettings model = CST.Security.PasswordSettings.GetPasswordSettings();
            return PartialView(model);
        }
        
        ///
        // GET: /Account/ChangeLanguage
        [HttpGet]
        public ActionResult ChangeLanguage()
        {
            string login = GetCurrentUserLogin();
            
            User user = GetUserByLogin(login);

            if (user == null)
            {
                return ErrMsgView("Failed to find user for current login "+ login);
            }
            
            ChangeLanguageModel model = new ChangeLanguageModel { Login = user.Login, Culture = user.Culture };
            return View(model);
        }

        ///
        // POST: /Account/ChangeLanguage
        [HttpPost]
        public ActionResult ChangeLanguage(ChangeLanguageModel model)
        {
            if (ModelState.IsValid)
            {
                User user = GetUserByLogin(model.Login);

                if (user != null)
                {
                    if (user.Culture != model.Culture)
                    {
                        user.Culture = model.Culture;
                        PrdnDBContext.SaveChanges();

                        SetSaveCurrentCulture(model.Culture);
                    }

                    return RedirectToAction("CurrentLanguage");
                }
                else {
                    ModelState.AddModelError(model.FullPropertyName(m => m.Login), "Invalid");
                }
            }
            
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult SetLanguage(string culture, string urlReturn)
        {
            SetSaveCurrentCulture(culture);
            return RedirectIfLocal(urlReturn, () => RedirectToAction("Index", "Home"));
        }

        ///
        // GET: /Account/CurrentLanguage
        [HttpGet]
        public ActionResult CurrentLanguage()
        {
            return View();
        }
    }
}
