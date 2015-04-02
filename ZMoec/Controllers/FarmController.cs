using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ZMoec.Models;

namespace ZMoec.Controllers
{
    /// <summary>
    /// This controller facilitates CRUD operations for farm
    /// </summary>
    public class FarmController : Controller
    {
        private OECContext db = new OECContext();

        /// <summary>
        /// Prepares the list of the farm
        /// </summary>
        /// <returns>View along with list of farm</returns>
        public ActionResult Index()
        {
            var farms = db.farms.Include(f => f.province).OrderBy(a => a.province.name).ThenBy(a => a.name);
            return View(farms.ToList());
        }

        /// <summary>
        /// Renders detail view of the farm
        /// </summary>
        /// <param name="id">Id of the requested detail of the farm</param>
        /// <returns>View along with farm model</returns>
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            farm farm = db.farms.Find(id);
            if (farm == null)
            {
                return HttpNotFound();
            }
            return View(farm);
        }

        /// <summary>
        /// Renders the view for creating farm
        /// </summary>
        /// <returns>Create farm view</returns>
        public ActionResult Create()
        {
            ViewBag.provinceCode = new SelectList(db.provinces, "provinceCode", "name");
            return View();
        }

        /// <summary>
        /// Save the data posted from the form in the database
        /// </summary>
        /// <param name="farm">Data posted from the form is received in this farm model</param>
        /// <returns>If errors then view along with posted farm data. Else redirects to Index page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "farmId,name,address,town,county,provinceCode,postalCode,homePhone,cellPhone,directions,dateJoined,lastContactDate")] farm farm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.farms.Add(farm);
                    db.SaveChanges();
                    TempData["message"] = "Farm created successfully";
                    TempData["messageType"] = "success";
                    return RedirectToAction("Index");
                   
                }
             
            }
            catch (Exception ex)
            {
                TempData["message"] = "Error creating farm: " + ex.GetBaseException().Message;
                TempData["messageType"] = "danger";

            }

            return View(farm);

        }

        /// <summary>
        /// Renders view for editing farm
        /// </summary>
        /// <param name="id">Id of the requested edit of the farm</param>
        /// <returns>View along with farm model</returns>
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            farm farm = db.farms.Find(id);
            if (farm == null)
            {
                return HttpNotFound();
            }
            ViewBag.provinceCode = new SelectList(db.provinces.OrderBy(a=>a.name), "provinceCode", "name", farm.provinceCode);
            return View(farm);
        }

        /// <summary>
        /// Saves the edited farm to the database
        /// </summary>
        /// <param name="farm">Updated farm model</param>
        /// <returns>If errors then view along with posted farm data. Else redirects to Index page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "farmId,name,address,town,county,provinceCode,postalCode,homePhone,cellPhone,directions,dateJoined,lastContactDate")] farm farm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(farm).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["message"] = "Farm " + farm.name + " update successfully";
                    TempData["messageType"] = "success";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = "Error updating farm: " + ex.GetBaseException().Message;
                TempData["messageType"] = "danger";
            }

            ViewBag.provinceCode = new SelectList(db.provinces.OrderBy(a => a.name), "provinceCode", "name", farm.provinceCode);
            return View(farm);
        }

        /// <summary>
        /// Renders confirmation page for deleting the farm
        /// </summary>
        /// <param name="id">Id of the requested deletion of the farm</param>
        /// <returns>View along with farm model</returns>
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            farm farm = db.farms.Find(id);
            if (farm == null)
            {
                return HttpNotFound();
            }
            return View(farm);
        }

        /// <summary>
        /// Upon confimation; deletes the farm from the database
        /// </summary>
        /// <param name="id">Id of the requested deletion of the farm</param>
        /// <returns>Redirects to Index page</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                farm farm = db.farms.Find(id);
                db.farms.Remove(farm);
                db.SaveChanges();
                TempData["message"] = "Farm deleted successfully";
                TempData["messageType"] = "success";
            }
            catch(Exception ex)
            {
                TempData["message"] = "Error deleting farm: " + ex.GetBaseException().Message;
                TempData["messageType"] = "danger";
            }
            
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Verifies if the user is entering correct province code and matches the one in the database
        /// </summary>
        /// <param name="provinceCode"></param>
        /// <returns></returns>
        public JsonResult checkProvinceCode(string provinceCode)
        {
            try
            {
                if (provinceCode.Length < 2 || provinceCode.Length > 2)
                    return Json("Province code should be 2 character long", JsonRequestBehavior.AllowGet);
                else if (db.provinces.Where(a => a.provinceCode == provinceCode.ToUpper()).Count() == 0)
                    return Json("Province code not on the file", JsonRequestBehavior.AllowGet);
                else
                    return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json("error validating provice code:" + ex.GetBaseException().Message, JsonRequestBehavior.AllowGet);
            }
        

        }
        /// <summary>
        /// Disposes all entity objects and closed connection with database
        /// </summary>
        /// <param name="disposing"></param>
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
