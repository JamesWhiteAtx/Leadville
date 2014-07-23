using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CST.Selector;
using RWSelector.Helpers;

namespace RWSelector.Controllers
{
    public class BigcoModel { 
        public string Email {get; set;}
        public string FirstName {get; set;}
        public string LastName {get; set;}
        public string Phone {get; set;}
        public string CarCD {get; set;}
        public string IntCD {get; set;}
        public string ProdCD {get; set;}
        public string Rows { get; set; }
    }

    public class BigcoController : Controller
    {
        private SelectorEntities _selectorContext = null;

        protected SelectorEntities SelectorContext
        {
            get
            {
                if (_selectorContext == null)
                {
                    _selectorContext = new SelectorEntities();
                }
                return _selectorContext;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_selectorContext != null)
            {
                _selectorContext.Dispose();
                _selectorContext = null;
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Seats()
        {
            return View();
        }

        [HttpPost]
        public ActionResult OrderSeat(BigcoModel model,  FormCollection col)
        {
            if (model.Rows == "1")
            {
                return Redirect("http://www.costco.com/.product.11756386.html");    
            }
            else if (model.Rows == "2")
            {
                return Redirect("http://www.costco.com/.product.11758427.html");
            }
            else // if (model.Rows == "2")
            {
                return Redirect("http://www.costco.com/.product.11499728.html");
            }
        }

        public ActionResult Makes()
        {
            var makes = (from m in SelectorContext.PubMakes()
                        select new {
                            id = m.MakeCD,
                            name = m.MakeDescription
                        }).ToList();
            return Json(makes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Years(string makeCD)
        {
            var years = (from y in SelectorContext.PubYears(makeCD)
                    select new
                    {
                        id = y.YearCD,
                        name = y.YearCD
                    }).ToList();
            return Json(years, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Models(string makeCD, decimal yearCD)
        {
            var models = (from m in SelectorContext.PubModels(makeCD, yearCD)
                         select new
                         {
                             id = m.ModelCD,
                             name = m.ModelDescription
                         }).ToList();
            return Json(models, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Bodies(string makeCD, decimal yearCD, string modelCD)
        {
            var bodies = (from b in SelectorContext.PubCars(makeCD, yearCD, modelCD)
                         select new
                         {
                             id = b.CarCD,
                             name = b.BodyDescription
                         }).ToList();
            return Json(bodies, JsonRequestBehavior.AllowGet);
        }

        const string CCCatalogCD = "CST";
        
        public ActionResult Patterns(string carCD)
        {
            var patterns = (from p in SelectorContext.Patterns(carCD, CCCatalogCD)
                          select new
                          {
                              id = p.PatternProdCD,
                              name = p.PatternDescription + " " + p.AirbagsDescription
                          }).ToList();
            return Json(patterns, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InteriorColors(string carCD, string patternCD)
        {
            var colors = (from c in SelectorContext.InteriorColors(carCD, CCCatalogCD, patternCD)
                          select new
                          {
                              id = c.InteriorColorCD,
                              name = c.InteriorColorDescription
                          }).ToList();
            return Json(colors, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LeatherColors(string carCD, string patternCD, string intColCD)
        {
            var colors = (from c in SelectorContext.ColorAdvices(carCD, CCCatalogCD, patternCD, intColCD)
                          select new
                          {
                              advice = c.Advice,
                              prodCD = c.ProdCD,
                              colorCD = c.ColorCD,
                              color = c.ColorDescription,
                              src = Url.CstColorImgUrl(c.ColorCD, c.ColorDescription).ToString()
                          }).ToList();
            
            

            return Json(colors, JsonRequestBehavior.AllowGet);
        }

        private string rowsDescr(int rows) {
            if (rows == 1)
            { return rows.ToString()+" row"; }
            else { return rows.ToString()+" rows"; }
        }

        public ActionResult Rows(string carCD, string patternCD)
        {
            var patterns = from p in SelectorContext.Patterns(carCD, CCCatalogCD)
                           select p;
            
            var rowCount = (from r in patterns
                       where r.PatternProdCD == patternCD
                       select new
                       {
                           r.NumberOfRows
                       }).FirstOrDefault();

            int rows = Convert.ToInt16(rowCount.IfNotNull(r => r.NumberOfRows));

            if (rows > 0)
            {
                var eachRow = new[] { new { id = rows.ToString(), name = rowsDescr(rows) } }.ToList();
                for (int i = (rows-1); i > 0; i--)
                {
                    eachRow.Add(new { id = i.ToString(), name = rowsDescr(i) });
                }

                return Json(eachRow, JsonRequestBehavior.AllowGet);
            }
            else {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
