using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Sockets;
using CST.Prdn.ViewModels;
using System.IO;
using System.Text;
using CST.ZebraUtils;
using System.Net;

namespace CST.Prdn.Controllers
{
    public class TestController : CstControllerBase
    {
        //defining ip address and port number        //Zebraclient.Connect("10.17.50.202", 6101);        //Zebraclient.Connect("10.17.50.202",9100);  
        protected string GetHostName()
        {
            return "192.168.1.132";
        }
        protected int GetPort()
        {
            return 9100;
        }

        [HttpGet]
        public ActionResult StatusNetwork()
        {
            IPEndPointTestModel model = new IPEndPointTestModel
            {
                HostName = GetHostName(),
                Port = GetPort()
            };
            return View(model);
        }
        
        [HttpPost]
        public ActionResult StatusNetwork(IPEndPointTestModel model)
        {
            model.Message = null;
            TcpClient Zebraclient = new TcpClient();
            try
            {
                Zebraclient.SendTimeout = 500;
                Zebraclient.ReceiveTimeout = 500;
                Zebraclient.Connect(model.HostName, model.Port);
                if (Zebraclient.Connected)
                {
                    //send and receive illustrated below
                    NetworkStream mynetworkstream;
                    StreamReader mystreamreader;
                    StreamWriter mystreamwriter;
                    mynetworkstream = Zebraclient.GetStream();
                    mystreamreader = new StreamReader(mynetworkstream);
                    mystreamwriter = new StreamWriter(mynetworkstream);
                    string commandtosend = "~HS";
                    mystreamwriter.WriteLine(commandtosend);
                    mystreamwriter.Flush();
                    char[] mk = null;
                    mk = new char[100];
                    mystreamreader.Read(mk, 0, mk.Length);
                    string data1 = new string(mk);
                    model.Message = "Response = "+data1;
                    Zebraclient.Close();
                }
                else
                {
                    ModelState.AddModelError("", "Not Connected");
                }
            }
            catch(Exception e)
            {
                ModelState.AddModelError("", e.Message);
            }

            return View(model);
        }

        protected string GetLabelMultiZpl()
        {
            ZplSetup setup = new ZplSetup(); // setup clear/default

            ZplCstMulti lbl1 = new ZplCstMulti
            {
                SerialNo = "111111",
                ProdCD = "101010",
                ProdDescr = "One Truck",
                ColorCD = "001",
                ColorDescr = "Unimer",
                DecoStr = "EPH",
                Note = "Notes",
                Pattern = "1111111-PT",
                EmbStr = "E-1018",
                HSStr = "H-3e3e",
                PerfStr = "P-4532",
                Priority = "Priority",
                PrdnOrder = "1523ALX"
            };
            ZplCstMulti lbl2 = new ZplCstMulti
            {
                SerialNo = "2222222",
                ProdCD = "20202",
                ProdDescr = "Second Truck",
                ColorCD = "002",
                ColorDescr = "Doesimer",
                DecoStr = "EPH",
                Note = "Notes",
                Pattern = "111111-PT",
                EmbStr = "E-1018",
                Priority = "Priority",
                PrdnOrder = "1523ALX"
            };
            ZplCstMulti lbl3 = new ZplCstMulti
            {
                SerialNo = "333986",
                ProdCD = "482822",
                ProdDescr = "Big red Truck",
                Note = "Notes",
                Pattern = "1234567-PT",
                Priority = "Priority",
                PrdnOrder = "1523ALX"
            };
            ZplClear clear = new ZplClear(); // clear after labels print

            StringBuilder sb = new StringBuilder();

            foreach (string instr in setup.Instructions)
            {
                sb.Append(instr);
            }

            sb.Append(Environment.NewLine);
            foreach (string instr in lbl1.Instructions)
            {
                sb.Append(instr);
            }
            sb.Append(Environment.NewLine);
            foreach (string instr in lbl2.Instructions)
            {
                sb.Append(instr);
            }
           
            sb.Append(Environment.NewLine);
            foreach (string instr in clear.Instructions)
            {
                sb.Append(instr);
            }

            return sb.ToString();
        }

        [HttpGet]
        public ActionResult LabelNetwork()
        {
            PrintLabelTestModel model = new PrintLabelTestModel
            {
                HostName = GetHostName(),
                Port = GetPort()
                //Zpl = GetLabelMultiZpl()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult LabelNetwork(PrintLabelTestModel model)
        {
            model.Message = null;

            NetworkStream ns = null;
            Socket socket = null;
            try
            {
                IPEndPoint printerIP = new IPEndPoint(IPAddress.Parse(GetHostName()),  GetPort());

                socket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);
                socket.Connect(printerIP);

                ns = new NetworkStream(socket);

                model.Zpl = GetLabelMultiZpl();
                byte[] toSend = Encoding.ASCII.GetBytes(model.Zpl);
                ns.Write(toSend, 0, toSend.Length);
                model.Message = "Label Sent!";
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
            }
            finally
            {
                if (ns != null)
                    ns.Close();

                if (socket != null && socket.Connected)
                    socket.Close();
            }
            return View(model);
        }

        public ActionResult LabelLocal()
        {
            return View();
        }

    }
}
