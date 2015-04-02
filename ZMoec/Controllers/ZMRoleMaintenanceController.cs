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
using Microsoft.AspNet.Identity.EntityFramework;

namespace ZMoec.Controllers
{
    /// <summary>
    /// This controller is used to manage roles in the system
    /// </summary>
    [Authorize(Roles = "Administrator")]//This controller is only accessible by Administrator
    public class ZMRoleMaintenanceController : Controller
    {
        private ApplicationUserManager _userManager;
        ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ZMRoleMaintenanceController()
        {
        }

        /// <summary>
        /// Instantiatin ApplicationUserManager class
        /// </summary>
        /// <param name="userManager"></param>
        public ZMRoleMaintenanceController(ApplicationUserManager userManager)
        {
            UserManager = userManager;

        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();//Getting ApplicationUserManager with Owin context. This is important to access some features like genereating reset password tokes etc.
            }
            private set
            {
                _userManager = value;
            }
        }

        /// <summary>
        /// Return list of Roles in the system
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            return View(db.Roles.OrderBy(a => a.Name).Select(a => a).ToList());
        }

        /// <summary>
        /// Creates new role
        /// </summary>
        /// <param name="form">Form collection that contains new role name</param>
        /// <returns>View along with errors or redirects to index if successfull</returns>
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            try
            {
                if (form["RoleName"].ToString().Trim() != "")
                {
                    IdentityRole model = new IdentityRole() { Name = form["RoleName"].ToString().Trim() };
                    if (db.Roles.Where(a => a.Name.Contains(model.Name)).Count() > 0)//Checking if role already exists
                    {

                        TempData["message"] = "Role - " + model.Name + " already exists";
                        TempData["messagetype"] = "danger";
                        return RedirectToAction("Index");
                    }
                    db.Roles.Add(model);
                    db.SaveChanges();
                    TempData["message"] = "Role - " + model.Name + " created successfully";
                    TempData["messagetype"] = "success";
                }
                else
                {
                    TempData["message"] = "Please enter Role";
                    TempData["messagetype"] = "danger";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["message"] = "Error creating Role: " + ex.GetBaseException().Message;
                TempData["messageType"] = "danger";
                return RedirectToAction("Index");
            }
        }



        /// <summary>
        /// Deletes the role directly if no users are associated with that role. Else will display list of users associated with the role and take confirmation from Admin to remove users from role
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(string id)
        {
            List<UserViewModel> list = new List<UserViewModel>();
            IdentityRole role = db.Roles.Find(id);
            ViewBag.roleName = role.Name;
            if (role.Name == "Administrator")
            {
                TempData["message"] = "Administrator role cannot be deleted";
                TempData["messageType"] = "danger";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You have to remove all the user from " + role.Name + " role before deleteing it";
                TempData["messageType"] = "info";
                var users = db.Users.Select(a => a);

                foreach (var a in users)
                {
                    UserViewModel tempModel = new UserViewModel();
                    if (UserManager.IsInRole(a.Id, role.Name))
                    {
                        tempModel.Email = a.Email;
                        tempModel.IsLockedOut = a.LockoutEnabled;
                        tempModel.UserId = a.Id;
                        tempModel.UserName = a.UserName;
                        list.Add(tempModel);
                    }

                }
            }
            if (list.Count() == 0)//if no users is associated with the role then it will delete the role directly
            {
                FormCollection form = new FormCollection();
                form["removeAllUsersFromRole"] = "true";
                form["roleName"] = ViewBag.roleName;
                Delete(form);
                return RedirectToAction("Index");
            }
            return View(list);
        }

        /// <summary>
        /// Removes all the users from the role and then deletes the role
        /// </summary>
        /// <param name="form">Form collection containing Admin confimation for removing users from role and then delete the role</param>
        /// <returns>Redirects to Index page upon successfull deletion</returns>
        [HttpPost]
        public ActionResult Delete(FormCollection form)
        {
            try
            {
                List<UserViewModel> list = new List<UserViewModel>();
                bool removeAllUsersFromRole = form["removeAllUsersFromRole"].ToString().Contains("true") ? true : false;
                string roleName = form["roleName"].ToString();
                var users = db.Users.Select(a => a);
                foreach (var a in users)
                {
                    UserViewModel tempModel = new UserViewModel();
                    if (UserManager.IsInRole(a.Id, roleName))
                    {
                        tempModel.Email = a.Email;
                        tempModel.IsLockedOut = a.LockoutEnabled;
                        tempModel.UserId = a.Id;
                        tempModel.UserName = a.UserName;
                        list.Add(tempModel);
                    }

                }
                ViewBag.roleName = roleName;
                if (roleName == "Administrator")//cannot delete administrator
                {
                    TempData["message"] = "You cannot delete Administrator role";
                    TempData["messageType"] = "danger";
                    return View(list);
                }


                if (removeAllUsersFromRole)//if checkedbox is checked
                {
                    var query = UserManager.Users.Select(a => a).ToList();
                    foreach (var a in query)
                    {
                        UserManager.RemoveFromRole(a.Id, roleName);
                    }
                    IdentityRole role = db.Roles.Where(a => a.Name == roleName).Select(a => a).SingleOrDefault();
                    db.Roles.Remove(role);
                    db.SaveChanges();
                    TempData["message"] = "Role - " + roleName + " deleted successfully";
                    TempData["messageType"] = "success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "You have to remove all the users from this role before deleting it";
                    TempData["messageType"] = "danger";

                }

                return View(list);


            }
            catch (Exception ex)
            {
                TempData["message"] = "Error deleting role:" + ex.GetBaseException().Message;
                TempData["messageType"] = "danger";
                return View();
            }
        }
        /// <summary>
        /// Displays all the users associated with the role
        /// </summary>
        /// <param name="id">role id</param>
        /// <returns>View along with list of users associated with the role</returns>
        public ActionResult Users(string id)
        {
            List<UserViewModel> usersInRole = new List<UserViewModel>();
            List<UserViewModel> usersNotInRole = new List<UserViewModel>();
            IdentityRole role = db.Roles.Find(id);
            ViewBag.currentUserRoles = UserManager.GetRoles(User.Identity.GetUserId()).ToList();
            ViewBag.roleName = role.Name;
            ViewBag.currentUserId = User.Identity.GetUserId();
            var users = db.Users.Select(a => a);
            foreach (var a in users)
            {
                UserViewModel tempModel = new UserViewModel();
                tempModel.Email = a.Email;
                tempModel.IsLockedOut = a.LockoutEnabled;
                tempModel.UserId = a.Id;
                tempModel.UserName = a.UserName;
                tempModel.Roles = UserManager.GetRoles(a.Id).ToList();
                if (UserManager.IsInRole(a.Id, role.Name))
                {

                    usersInRole.Add(tempModel);
                }
                else
                {
                    usersNotInRole.Add(tempModel);
                }

            }
            ViewBag.userId = new SelectList(usersNotInRole, "UserId", "Email");
            return View(usersInRole);
        }
        /// <summary>
        /// Will add user to role
        /// </summary>
        /// <param name="form">form collection containing userid and role name</param>
        /// <returns>Redirects to User listing of the role</returns>
        [HttpPost]
        public ActionResult AddUserToRole(FormCollection form)
        {
            string roleName = form["roleName"].ToString().Trim();
            var query = db.Roles.Where(a => a.Name.Equals(roleName)).SingleOrDefault();
            try
            {

                UserManager.AddToRole(form["userId"], form["roleName"]);
                TempData["message"] = "User added to " + form["roleName"].ToString() + " Role successfully";
                TempData["messageType"] = "success";


            }
            catch (Exception ex)
            {
                TempData["message"] = "Error adding user to role: " + ex.GetBaseException().Message;
                TempData["messageType"] = "danger";
            }
            return RedirectToAction("Users", new { id = query.Id });
        }

        /// <summary>
        /// Romoves user from role
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="roleName"> role name</param>
        /// <returns>Redirects to User listings page</returns>
        public ActionResult RemoveUserFromRole(string id, string roleName)
        {
            var query = db.Roles.Where(a => a.Name.Equals(roleName)).SingleOrDefault();
            try
            {

                UserManager.RemoveFromRole(id, roleName);
                TempData["message"] = "User removed from " + roleName + " Role successfully";
                TempData["messageType"] = "success";


            }
            catch (Exception ex)
            {
                TempData["message"] = "Error removing user from role: " + ex.GetBaseException().Message;
                TempData["messageType"] = "danger";
            }
            return RedirectToAction("Users", new { id = query.Id });
        }
    }
}
