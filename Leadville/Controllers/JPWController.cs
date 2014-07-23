using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Http;
//using System.Web.Http;
using System.IO;
using System.Net.Http.Headers;

namespace CST.Prdn.Controllers
{
    public class JPWController : Controller
    {
        //
        // GET: /JPW/

        public ActionResult Index()
        {

            ViewBag.Msg = "xx http://leadville.classicsofttrim.com/";

            try
            {
                string method = "GET";
                WebHeaderCollection headers = null;

                //WebRequest request = WebRequest.Create(uri);
                WebRequest request = WebRequest.Create("http://leadville.classicsofttrim.com/");
                request.Method = method;
                if (headers != null)
                {
                    foreach (string name in headers)
                    {
                        request.Headers.Add(name, headers[name]);
                    }
                }

                using (WebResponse respFromServer = request.GetResponse())
                {
                    using (Stream dataStream = respFromServer.GetResponseStream())
                    {
                        
                        MemoryStream ms = new MemoryStream();
                        dataStream.CopyTo(ms);

                        HttpResponseMessage response = new HttpResponseMessage();
                        response.StatusCode = HttpStatusCode.OK;
                        ms.Position = 0;
                        response.Content = new StreamContent(ms);
                        response.Content.Headers.Add("Content-Type", respFromServer.ContentType);
                        
                        //return response;
                    }
                }
            }
            catch (WebException webEx)
            {
                string msg = "webEx.ToString() = " + webEx.Status.ToString() + "| " + webEx.ToString();
                // Now you can access webEx.Response object that contains more info on the server response              
                if (webEx.Status == WebExceptionStatus.ProtocolError)
                {
                    msg = ((HttpWebResponse)webEx.Response).StatusCode.ToString()
                        + " - " + ((HttpWebResponse)webEx.Response).StatusDescription.ToString();
                }

                ViewBag.Msg = msg;

            }
            catch (Exception e)
            {
                ViewBag.Msg = "jpw err " + e.ToString();
            }

            
            return View();
        }

    }
}
