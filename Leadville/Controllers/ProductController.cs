using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;
using CST.ISIS.Data;
using CST.Prdn.ISIS;
using CST.Prdn.Helpers.Extensions;

namespace CST.Prdn.Controllers
{
    public class ProductController : CstControllerBase
    {
        public ActionResult _ProdCdLookup(string prodCd, string prodType=null, int? take=null)
        {
            var prodList = from p in PrdnDBContext.Products
                           where p.ProdCD.StartsWith(prodCd)
                           orderby p.ProdCD
                           select new ProductListModel
                           {   ProdTypeCD = p.ProdTypeCD,
                               ProdTypeDescr = p.ProductType.Description,
                               ProdCD = p.ProdCD,
                               ProdDescr = p.Description,
                               ParentProdCD = p.ParentProdCD,
                               Cost = p.Cost
                           };
            if (!String.IsNullOrWhiteSpace(prodType))
            {
                prodList = from p in prodList
                           where p.ProdTypeCD == prodType
                           select p;
            }
            if ((take ?? 0) > 0)
            {
                prodList = prodList.Take((int)take);
            }

            return Json(prodList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ProdCodeCheck(string prodCd, string prodType=null)
        {
            var prodQry = from p in PrdnDBContext.Products
                          where (p.ProdCD == prodCd)
                          select new ProductListModel
                          {   ProdTypeCD = p.ProdTypeCD,
                              ProdTypeDescr = p.ProductType.Description,
                              ProdCD = p.ProdCD,
                              ProdDescr = p.Description,
                              ParentProdCD = p.ParentProdCD,
                              Cost = p.Cost
                          };

            if (!String.IsNullOrWhiteSpace(prodType))
            {
                prodQry = from p in prodQry
                          where p.ProdTypeCD == prodType
                          select p;
            }

            var prodObj = (from p in prodQry select p).FirstOrDefault();

            if (prodObj == null)
            {
                return Json(prodObj, JsonRequestBehavior.AllowGet);    
            }

            ProductLookupModel model = ProductSubViews(prodObj.ProdCD, prodObj.ParentProdCD, prodObj);
           
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        protected ProductLookupModel ProductSubViews(string prodCD, string parentProdCD, IProductCostViewModel product = null)
        {
            List<ProdOptDefn> prodOptions = ProdOptDefn.ProdOptions(prodCD, parentProdCD);
            List<ProdImageInfo> prodImageInfoSet = IsisDbContext.ProdImageInfoSet(prodCD);

            ProductLookupModel model = new ProductLookupModel
            {
                OptsPartial = RenderPartialView("CharCompOpts", prodOptions),
                ImgSetPartial = RenderPartialView("ProdImageInfoSet", prodImageInfoSet)
            };

            if (product != null)	
            {
                model.ProdTypeCD = product.ProdTypeCD;
                model.ProdTypeDescr = product.ProdTypeDescr;
                model.ProdCD = product.ProdCD;
                model.ProdDescr = product.ProdDescr;
                model.ParentProdCD = product.ParentProdCD;
                model.Cost = product.Cost;
            }

            return model;
        }

        public ActionResult _ProdSubViews(string prodCD, string parentProdCD)
        {
            ProductLookupModel model = ProductSubViews(prodCD, parentProdCD);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ProdImageInfoSetPartial(string prodCD)
        {
            List<ProdImageInfo> prodImageInfoSet = IsisDbContext.ProdImageInfoSet(prodCD);
            return PartialView("ProdImageInfoSet", prodImageInfoSet);
        }

        public ActionResult _ProdImgSet(string prodCD)
        {
            List<ProdImageInfo> prodImageInfoSet = IsisDbContext.ProdImageInfoSet(prodCD);

            var imgSet = from info in prodImageInfoSet
                      select new {
                        info.ImageID, 
                        info.ImageType,
                        SrcPath = Url.ProdImageSetSrcPath(info.ImageSetCD)
                      };

            return Json(imgSet, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewImage(int id)
        {
            ImageFile imgFile = IsisDbContext.ImageFile(id);

            return ViewAttachment(imgFile);
        }

        //public ActionResult _ProdOptions(string prodCD, string parent)
        //{
        //    List<ProdOptDefn> prodOptions = ProdOptDefn.ProdOptions(prodCD, parent);
        //    return Json(prodOptions, JsonRequestBehavior.AllowGet);
        //}

    }
}
