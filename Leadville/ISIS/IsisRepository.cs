using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CST.ISIS.Data;

namespace CST.Prdn.ISIS
{
    public static class IsisRepository
    {
        //private static IsisEntities isisContext = null;
        public static IsisEntities CreateIsisContext()
        {
            //if (isisContext == null) {                isisContext = new IsisEntities("name=IsisEntities");            }
            return new IsisEntities("name=IsisEntities");
        }
    }

}