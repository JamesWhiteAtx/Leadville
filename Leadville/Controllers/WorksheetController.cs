using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;
using CST.ISIS.Data;

namespace CST.Prdn.Controllers
{
    public class WorksheetController : CstControllerBase
    {
        private PartialViewResult WorksheetRowsPartial(IEditWorksheetOwner model)
        {
            string optsKey = model.FullPropertyName(m => m.EditWorksheet.WorksheetOpts);
            foreach (var delPair in ModelState.Where(p => (p.Key.Contains(optsKey))).ToList())
            {
                ModelState.Remove(delPair.Key);
            }

            string modKey = model.FullPropertyName(m => m.EditWorksheet.Modified);
            ModelState.Remove(modKey);

            return PartialView("WorksheetRows", model);
        }

        public PartialViewResult Meta(EditWorksheetOwner model)
        {
            if (model.EditWorksheet != null)
            {
                model.EditWorksheet.WorksheetMeta();
                assignOptImgs(model);
            }

            return WorksheetRowsPartial(model);
        }

        public PartialViewResult AddComp(EditWorksheetOwner model)
        {
            if (model.EditWorksheet != null)
            {
                assignOptImgs(model);

                WorksheetEditOpt opt = model.EditWorksheet.AddComp();
                AssgnOptImgs(opt);
            }

            return WorksheetRowsPartial(model);
        }

        public PartialViewResult WorksheetPartial(EditWorksheetOwner model)
        {
            string wsKey = model.FullPropertyName(m => m.EditWorksheet);
            foreach (var delPair in ModelState.Where(p => (p.Key.Contains(wsKey))).ToList())
            {
                ModelState.Remove(delPair.Key);
            }

            assignOptImgs(model);

            return PartialView("Worksheet", model);
        }

        protected void assignOptImgs(EditWorksheetOwner model)
        {
            if (model.NotNullAny(m => m.IfNotNull(e => e.EditWorksheet).IfNotNull(w => w.WorksheetOpts)))
            {
                foreach (var opt in model.EditWorksheet.WorksheetOpts.Where(o => (o.Type == OptionType.Component) && (o.ImgCount > 0)))
                {
                    AssgnOptImgs(opt);
                }
            }
        }

        protected void AssgnOptImgs(WorksheetEditOpt opt)
        {
            opt.AssignTypeProps();
            opt.ProdImageInfoSet = IsisDbContext.ProdImageInfoSet(opt.CompProdCD);
            opt.ImgCount = opt.ProdImageInfoSet.IfNotNull(s => s.Count);
        }

        protected IQueryable<Product> ProdsForType(string prodTypeCD, bool includeImgs)
        {
            DateTime now = DateTime.Now;
            System.Data.Objects.ObjectQuery<Product> prods = PrdnDBContext.Products;
            if (includeImgs)
            {
                prods = prods.Include("ProdImages");
            }
            return from p in prods
                        where p.ProdTypeCD == prodTypeCD && (p.DiscontinueDt == null || p.DiscontinueDt > now)
                        orderby p.ProdCD
                        select p;
        }

        public ActionResult _WSCompList(string prodType)
        {
            var prods = from p in ProdsForType(prodType, true)
                        select new {
                            p.ProdCD,
                            p.Description,
                            p.UserTextFlag,
                            p.ProdImages
                        };

            var prodImgIds = from p in prods.ToList()
                         select new {
                             p.ProdCD,
                             p.Description,
                             Display = p.ProdCD + " - " + p.Description,
                             UserDefined = (p.UserTextFlag == PrdnDataHelper.BoolYNTue),
                             ImageCount = p.IfNotNull(x => x.ProdImages).IfNotNull(i => i.Count),
                             ImageID = p.IfNotNull(x => x.ProdImages).IfNotNull(i => i.FirstOrDefault(s => IsisEntities.ImgSetCds.Contains(s.ImageSetCD))).IfNotNull(i => i.ImageID)
                         };

            return Json(prodImgIds, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OptionLookup(CST.ISIS.Data.OptionType optType, string optTypeCD, string term)
        {
            if (optType == OptionType.Characteristic)
            {
                return JsonCharValues(optTypeCD, term);
            }
            else if (optType == OptionType.Component)
            {
                return JsonCompValues(optTypeCD, term);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult JsonCharValues(string charTypeCD, string term)
        {
            string charValCD = term.Trim().ToUpper();

            var prods = (from c in PrdnDBContext.ProdChars
                         where (
                            c.ProdCharTypeCD == charTypeCD 
                            && c.ProdCharValueCD.StartsWith(charValCD)
                            && c.StatusFlag == PrdnDataHelper.StatusActive
                         )
                         orderby c.SortOrder, c.ProdCharValueCD
                         select new
                         {
                             LookupValue = c.ProdCharValueCD,
                             LookupLabel = c.ProdCharValueCD + " - " + c.Description,
                             OptionCode2 = c.ProdCharValueCD,
                             OptionCode3 = c.ProdCharCD,
                             OptionDescr = c.Description,
                             UserDefined = (c.UserDefinedFlag == PrdnDataHelper.BoolYNTue)
                         }).Take(20);

            return Json(prods, JsonRequestBehavior.AllowGet);
        }

        public JsonResult JsonCompValues(string prodTypeCD, string term)
        {
            string prodCd = term.Trim().ToUpper(); 
            
            var prods = (from p in ProdsForType(prodTypeCD, false)
                         where p.ProdCD.StartsWith(prodCd)
                         select new
                         {
                             LookupValue = p.ProdCD,
                             LookupLabel = p.ProdCD + " - " + p.Description,
                             OptionCode2 = p.ProdCD,
                             OptionCode3 = String.Empty,
                             OptionDescr = p.Description,
                             UserDefined = (p.UserTextFlag == PrdnDataHelper.BoolYNTue)
                         }).Take(20);

            return Json(prods, JsonRequestBehavior.AllowGet);
        }

    }
}
