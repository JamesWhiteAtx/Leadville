using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects;
using System.Web.Mvc;
using CST.ISIS.Data;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;

namespace CST.Prdn.ISIS
{
    public static class PrdnIsisEntities
    {
        public static IQueryable<SalesDept> CustDepts(this IsisEntities isis)
        {
            var depts = from d in isis.SalesDepts
                        where d.DeptID != PrdnDataHelper.RWIsisDeptID
                        orderby d.DeptID
                        select d;

            //if (custID == PrdnDataHelper.PrdnCustIDRW)
            //{
            //    depts = from d in depts
            //            where d.DeptID == PrdnDataHelper.RWIsisDeptID
            //            select d;
            //}
            //else
            //{
            //    depts = from d in depts
            //            where d.DeptID != PrdnDataHelper.RWIsisDeptID
            //            select d;
            //}

            //depts = from d in depts
            //        orderby d.DeptID
            //        select d;

            return depts;
        }

        public static IQueryable<CustLocViewModel> CustLocs(this IsisEntities isis)
        {
            var depts = CustDepts(isis);

            if (depts == null)
            {
                return null;//new SelectList(new List<string>());
            }
            else
            {
                var locs = from d in depts
                           orderby d.DeptID
                           select new CustLocViewModel {    
                               LocID = d.DeptID,
                               Description = d.Description,
                               Display = d.DeptID + "-" + d.Description
                           };

                return locs;
            }
        }
        
        public static SelectList CustLocsSelList(this IsisEntities isis)
        {
            var locs = CustLocs(isis);

            if (locs == null)
            {
                return new SelectList(new List<CustLocViewModel>());
        	} 
            else
            {
                return new SelectList(locs.ToList(), "LocID", "Display");
            }
        }

        public static decimal GetNextWorksheetID()
        {
            return IsisDataHelper.SeqNextVal("FG_WORKSHEET_ID_SEQ");
        }

    }
}