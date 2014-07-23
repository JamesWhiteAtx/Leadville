using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CST.Prdn.ViewModels
{
    public class CstErrorModel
    {
        public CstErrorModel()
        {
            message = String.Empty;
        }

        public CstErrorModel(string msg)
        {
            message = msg;
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

    }
}