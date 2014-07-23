using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CST.Leadville.Models;

namespace CST.Leadville.Controllers
{ 
    public class RunController : Controller
    {
        private LeadvilleEntities db = new LeadvilleEntities();

        //
        // GET: /Run/

        public ViewResult Index()
        {
            var productionruns = db.ProductionRuns.Include("ProductionOrder").Include("ProductionType");
            return View(productionruns.ToList());
        }

        //
        // GET: /Run/Details/5

        public ViewResult Details(int id)
        {
            ProductionRun productionrun = db.ProductionRuns.Single(p => p.ProductionRunID == id);
            return View(productionrun);
        }

        //
        // GET: /Run/Create

        public ActionResult Create()
        {
            ViewBag.ProductionOrdNo = new SelectList(db.ProductionOrders, "ProductionOrdNo", "ProductionOrdNo");
            ViewBag.TypeID = new SelectList(db.ProductionTypes, "TypeID", "TypeCode");
            return View();
        } 

        //
        // POST: /Run/Create

        [HttpPost]
        public ActionResult Create(ProductionRun productionrun)
        {
            if (ModelState.IsValid)
            {
                db.ProductionRuns.AddObject(productionrun);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.ProductionOrdNo = new SelectList(db.ProductionOrders, "ProductionOrdNo", "ProductionOrdNo", productionrun.ProductionOrdNo);
            ViewBag.TypeID = new SelectList(db.ProductionTypes, "TypeID", "TypeCode", productionrun.TypeID);
            return View(productionrun);
        }
        
        //
        // GET: /Run/Edit/5
 
        public ActionResult Edit(int id)
        {
            ProductionRun productionrun = db.ProductionRuns.Single(p => p.ProductionRunID == id);
            ViewBag.ProductionOrdNo = new SelectList(db.ProductionOrders, "ProductionOrdNo", "ProductionOrdNo", productionrun.ProductionOrdNo);
            ViewBag.TypeID = new SelectList(db.ProductionTypes, "TypeID", "TypeCode", productionrun.TypeID);
            return View(productionrun);
        }

        //
        // POST: /Run/Edit/5

        [HttpPost]
        public ActionResult Edit(ProductionRun productionrun)
        {
            if (ModelState.IsValid)
            {
                db.ProductionRuns.Attach(productionrun);
                db.ObjectStateManager.ChangeObjectState(productionrun, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProductionOrdNo = new SelectList(db.ProductionOrders, "ProductionOrdNo", "ProductionOrdNo", productionrun.ProductionOrdNo);
            ViewBag.TypeID = new SelectList(db.ProductionTypes, "TypeID", "TypeCode", productionrun.TypeID);
            return View(productionrun);
        }

        //
        // GET: /Run/Delete/5
 
        public ActionResult Delete(int id)
        {
            ProductionRun productionrun = db.ProductionRuns.Single(p => p.ProductionRunID == id);
            return View(productionrun);
        }

        //
        // POST: /Run/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            ProductionRun productionrun = db.ProductionRuns.Single(p => p.ProductionRunID == id);
            db.ProductionRuns.DeleteObject(productionrun);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}