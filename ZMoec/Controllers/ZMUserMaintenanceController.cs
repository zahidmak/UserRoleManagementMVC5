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
using ZMoec.Models;
using System.Collections.Generic;
using System.Data.Entity;

namespace ZMoec.Controllers
{
    /// <summary>
    /// This controller is used to manage users in the system
    /// </summary>
    [Authorize(Roles = "Administrator")]//This controller is only accessible to Administrator
    public class ZMUserMaintenanceController : Controller
    {
        private ApplicationUserManager _userManager;
        ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Default constructor
        /// </summary>
        public ZMUserMaintenanceController()
        {
        }

        /// <summary>
        /// Constructor that makes instance of ApplicationUserManager available for use in rest of the controller
        /// </summary>
        /// <param name="userManager"></param>
        public ZMUserMaintenanceController(ApplicationUserManager userManager)
        {
            UserManager = userManager;

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



        /// <summary>
        /// Return the View with list of all the users in the system
        /// </summary>
        /// <returns>View along with list of users</returns>
        public ActionResult Index()
        {
            var users = db.Users.Select(a => a);
            List<UserViewModel> list = new List<UserViewModel>();

            foreach (var a in users)
            {
                UserViewModel model = new UserViewModel();
                model.Email = a.Email;
                model.UserId = a.Id;
                model.UserName = a.UserName;
                model.IsLockedOut = a.LockoutEnabled;
                model.IsAdmin = UserManager.GetRoles(a.Id).Contains("Administrator") ? true : false;
                var logins = UserManager.GetLogins(a.Id).ToList();
                if (logins.Count() > 0)
                {
                    model.IsAuthorizedLocally = false;
                }
                list.Add(model);
            }
            return View(list.OrderBy(a => a.IsLockedOut).ThenBy(a => a.UserName));
        }

        /// <summary>
        /// Toggles the lock associated with the user
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>Redirects to index page on sucessfull toggle of lock</returns>
        public ActionResult ToggleLock(string id)
        {
            ApplicationUser user = UserManager.FindById(id);
            try
            {

                UserManager.SetLockoutEnabled(id, !(user.LockoutEnabled));
                ApplicationUser tempUser = db.Users.Where(a => a.Id == id).SingleOrDefault();
                tempUser.LockoutEndDateUtc = null; //inserting null LockoutEndDateUtc
                db.Users.Attach(tempUser);
                db.Entry(tempUser).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "User - " + user.UserName + ((user.LockoutEnabled == true) ? " Locked " : " Unlocked ") + "successfully";
                TempData["messageType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["message"] = "Error " + ((!user.LockoutEnabled == true) ? " Locking " : " Unlocking ") + " User :" + ex.GetBaseException().Message;
                TempData["messageType"] = "danger";
            }

            return RedirectToAction("Index");

        }
        /// <summary>
        /// Renders view with ResetUserPasswordViewModel to reset the password of the user
        /// </summary>
        /// <param name="id">User id of the user whose password is to be reseted</param>
        /// <returns></returns>
        public ActionResult ResetPassword(string id)
        {
            ResetUserPasswordViewModel model = new ResetUserPasswordViewModel();
            model.Code = UserManager.GeneratePasswordResetToken(id);
            ApplicationUser user = UserManager.FindById(id);
            model.Email = user.Email;
            return View(model);
        }

        /// <summary>
        /// Takes the new password from the view and saves it to the database
        /// </summary>
        /// <param name="model">ResetUserPasswordViewModel</param>
        /// <param name="id">User id of the user whose password is to be reseted</param>
        /// <returns>Redirects to Index page upon successful reset of password</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetUserPasswordViewModel model, string id)
        {
            var user = UserManager.FindById(id);
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await UserManager.ResetPasswordAsync(id, model.Code, model.Password);
                if (result.Succeeded)
                {
                    TempData["message"] = "Password for - " + user.UserName + " changed successfully";
                    TempData["messageType"] = "success";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = "Error changing password for - " + user.UserName + " " + ex.GetBaseException().Message;
                TempData["messageType"] = "danger";
            }

            return View();
        }

        /// <summary>
        /// Renders the detail view of the user for confirming the deletion of the user
        /// </summary>
        /// <param name="id">User id of the user who is to be deleted</param>
        /// <returns>View along with user model</returns>
        public ActionResult Delete(string id)
        {
            ApplicationUser user = UserManager.FindById(id);
            UserViewModel model = new UserViewModel() { Email = user.Email, UserId = user.Id, UserName = user.UserName, IsAuthorizedLocally = UserManager.GetLogins(id).Count() > 0 ? false : true };
            return View(model);
        }

        /// <summary>
        /// Deletes the user from the system
        /// </summary>
        /// <param name="id">User id of the user who is to be deleted</param>
        /// <param name="collection"></param>
        /// <returns>View along with user model</returns>
        [HttpPost]
        public ActionResult Delete(string id, FormCollection collection)
        {
            ApplicationUser user = UserManager.FindById(id);
            try
            {
                UserManager.RemoveFromRoles(id, UserManager.GetRoles(id).ToArray());
                UserManager.Delete(user);

                TempData["message"] = "User - " + user.Email + " deleted successfully";
                TempData["messageType"] = "success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["message"] = "Error deleting User:" + ex.GetBaseException().Message;
                TempData["messageType"] = "danger";
                return View(user);
            }
        }
    }
}
