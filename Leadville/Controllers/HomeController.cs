using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Objects;
using Telerik.Web.Mvc;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;
using AutoMapper;
using CST.Prdn.ISIS;
using CST.ISIS.Data;
using System.Web.UI;

namespace CST.Prdn.Controllers
{
    public class HomeController : CstControllerBase
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            IndexModel model = new IndexModel();
            if (Request.IsAuthenticated)
            {
                model.SettingsModel = GetUserSettingsViewModel(false);
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.DBName = PrdnDBContext.DBNameObfuscate();
            return View();
        }

    }
}
