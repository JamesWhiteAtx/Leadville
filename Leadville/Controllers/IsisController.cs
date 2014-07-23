using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CST.ISIS.Data;
using CST.Prdn.ISIS;
using CST.Prdn.ViewModels;

namespace CST.Prdn.Controllers
{
    public class IsisController : CstControllerBase
    {
        public ActionResult _CustShipAddrLookup(string isisCustID)
        {
            var addrs = from c in IsisDbContext.SalesCustAddrs
                        where (c.CustID.StartsWith(isisCustID))
                        orderby c.CustID
                        select new CustLookupViewModel
                        {
                            CustID = c.CustID,
                            CustName = c.Name1,
                            Addr1 = c.ShipAddr1,
                            Addr2 = c.ShipAddr2,
                            Addr3 = c.ShipAddr3,
                            Addr4 = c.ShipAddr4,
                            City = c.ShipCity,
                            State = c.ShipState,
                            Postal = c.ShipPostal,
                            Country = c.ShipCountry
                        };
            
            return Json(addrs, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _CustIDShipAddrLookup(string isisCustID)
        {
            var addr = (from c in IsisDbContext.SalesCustAddrs
                        where c.CustID == isisCustID
                        select new CustLookupViewModel 
                        {   CustID = c.CustID,
                            CustName = c.Name1,
                            Addr1 = c.ShipAddr1,
                            Addr2 = c.ShipAddr2,
                            Addr3 = c.ShipAddr3,
                            Addr4 = c.ShipAddr4,
                            City = c.ShipCity,
                            State = c.ShipState,
                            Postal = c.ShipPostal,
                            Country = c.ShipCountry
                        }).FirstOrDefault();

            return Json(addr, JsonRequestBehavior.AllowGet);
        }
    }
}
