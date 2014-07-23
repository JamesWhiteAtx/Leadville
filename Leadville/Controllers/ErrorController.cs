using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CST.Prdn.ViewModels;

namespace CST.Prdn.Controllers
{
    public class ErrorController : CstControllerBase
    {
        public ActionResult Message(string message)
        {
            return View( new CstErrorModel(message));
        }
    }
}
