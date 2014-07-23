using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;
using System.Data;
using NPOI.HSSF.UserModel;
using CST.ActionFilters;

namespace CST.Prdn.Controllers
{
    [CstAuthorize(Groups = "UTIL/UTIL")]
    public class UtilController : JobControllerBase
    {
        private UtilEntities _utilDbContext = null;

        protected UtilEntities UtilDBContext
        {
            get
            {
                if (_utilDbContext == null)
                {
                    _utilDbContext = new UtilEntities();
                }
                return _utilDbContext;
            }
        }

        protected void EnsureUtilDBContextOpen()
        {
            if (UtilDBContext.Connection.State != ConnectionState.Open)
            {
                UtilDBContext.Connection.Open();
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AutoWidget()
        {
            return View();
        }

        [HttpGet]
        public ActionResult NSSelector()
        {
            return View();
        }

        protected IQueryable<BulkSaleImpSumModel> BulkRuns()
        {
            return (from i in UtilDBContext.BulkSaleImports
                    group i by new { i.RunName, i.LockedStr } into grp
                    select new BulkSaleImpSumModel
                    {
                        RunName = grp.Key.RunName,
                        LockedStr = grp.Key.LockedStr,
                        Count = grp.Count()
                    }).OrderBy(i => i.LockedStr).ThenBy(i => i.RunName);
        }

        [HttpGet]
        public ActionResult TrnsfrWrhsList()
        {
            var model = BulkRuns().ToList();
            return View(model);
        }

        [HttpGet]
        public ActionResult EditWrhse(string id)
        {
            CountUnLocked(id);
            BulkSaleImpIModel model = new BulkSaleImpIModel { RunName = id };
            return View(model);
        }

        protected int CountUnLocked(string id)
        {
            if (id == null)
            {
                return 0;
            }

            IQueryable<BulkSaleImpSumModel> bulkRuns = BulkRuns();

            var runs = (from r in bulkRuns
                        where r.RunName == id 
                        select r).ToList();

            if (runs.Where(r => r.Locked).Any())
            {
                throw new Exception("Warehouse " + id + " is locked!");
            }

            var run = runs.FirstOrDefault();
            if (run == null)
            {
                return 0;
            }
            else {
                return run.Count;
            }
        }

        [HttpGet]
        public ActionResult UploadWrhse(string id)
        {
            CountUnLocked(id);

            BulkSaleImpIModel model = new BulkSaleImpIModel();
         
            if (String.IsNullOrWhiteSpace(id)) {
                model.Count = 0; 
            } 
            else {
                model.RunName = id;
                model.Count = (from i in UtilDBContext.BulkSaleImports
                         where i.RunName == id
                         select i.RunName).Count();
            }
            model.Insert = (model.Count < 1);

            return View(model);
        }

        [HttpPost]
        public ActionResult UploadWrhse(BulkSaleImpIModel model, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            CountUnLocked(model.IfNotNull(m => m.RunName));

            HttpPostedFileBase file = uploadedFiles.IfNotNull(f => f.First());
            if (file == null)
            {
                ModelState.AddModelError("", "No file was uploaded");
            }
            if (ModelState.IsValid)
            {
                Stream stream = file.InputStream;
                StreamReader sr = new StreamReader(stream);
                
                int rowCount = 0;
                if (model.SkipFirstRow)
                {
                    sr.ReadLine();
                }
                string line;
                string[] row;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        row = line.Trim().Split(',');

                        BulkSaleImport newImport = new BulkSaleImport();
                        newImport.RunName = model.RunName;
                        newImport.SerialNum = row[0];
                        if (row.Length > 1)
                        {
                            newImport.CustPO = row[1];
                        }

                        BulkSaleImport existImport = null;
                        if (!model.Insert)
                        {
                            existImport = (from i in UtilDBContext.BulkSaleImports
                                           where i.RunName == newImport.RunName && i.SerialNum == newImport.SerialNum
                                           select i).FirstOrDefault();
                        }
                        if (existImport == null)
                        {
                            UtilDBContext.BulkSaleImports.AddObject(newImport);
                        }
                        else
                        {
                            existImport.CustPO = newImport.CustPO;
                        }
                        
                        rowCount++;
                    }
                }
                if (rowCount > 0)
                {
                    UtilDBContext.SaveChanges();
                    return RedirectToAction("EditWrhse", new {id = model.RunName}); 
                }
                else 
                    ModelState.AddModelError("", "No rows were imported");
            }
            return View(model);
        }

        protected IQueryable<BulkSaleImportInfo> BulkSaleImpList(string id)
        {
            var items = from i in UtilDBContext.BulkSaleImportInfoes
                        where i.RunName == id
                        orderby i.CustPO, i.VendorID, i.ProdCD, i.SerialNum
                        select i;
            return items;
        }

        protected IQueryable<BulkSaleImpInfo> SerialPoViewModelList(string id, bool locked=false)
        {
            string lockStr = locked ? "T" : "F";

            var items = from x in BulkSaleImpList(id)
                        where x.LockedStr == lockStr 
                        select new BulkSaleImpInfo
                        {
                            RunName = x.RunName,
                            SerialNum = x.SerialNum,
                            CustPO = x.CustPO,
                            VendorID = x.VendorID,
                            ProdCD = x.ProdCD,
                            ProdDescr = x.ProdDescr,
                            HV = x.HV,
                            DeptID = x.DeptID,
                            Status = x.Status,
                            Cost = x.Cost
                        };
            return items;
        }

        protected GridModel UnlockedSerialPoGridList(string id)
        {
            return new GridModel(SerialPoViewModelList(id, false));
        }

        [GridAction]
        public ActionResult _SelectWhtrans(string id)
        {
            var x = View(UnlockedSerialPoGridList(id));
            return x;
        }

        [HttpPost]
        [GridAction]
        public ActionResult _SaveWhtrans(string id, BulkSaleImpItem editModel)
        {
            if (String.IsNullOrWhiteSpace(editModel.SerialNum))
            {
                ModelState.AddModelError(editModel.FullPropertyName(m => m.SerialNum), "Required"); 
            }
            else
            {
                BulkSaleImpItem idModel = new BulkSaleImpItem();
                if (idModel.ParseItemKey(id))
                {
                    BulkSaleImport import = (from i in UtilDBContext.BulkSaleImports
                                             where i.RunName == idModel.RunName && i.SerialNum == idModel.SerialNum
                                             select i).FirstOrDefault();
                    if (import != null)
                    {
                        if (import.SerialNum == editModel.SerialNum)
                        {
                            if (TryUpdateModel(import))
                            {
                                UtilDBContext.SaveChanges();
                            }
                        }
                        else
                        {
                            BulkSaleImport newImport = new BulkSaleImport();
                            if (TryUpdateModel(newImport))
                            {
                                BulkSaleImport existImport = (from i in UtilDBContext.BulkSaleImports
                                                              where i.RunName == newImport.RunName && i.SerialNum == newImport.SerialNum
                                                              select i).FirstOrDefault();
                                if (existImport == null)
                                {
                                    //Delete
                                    UtilDBContext.DeleteObject(import);
                                    UtilDBContext.SaveChanges();
                                    //insert
                                    UtilDBContext.BulkSaleImports.AddObject(newImport);
                                    UtilDBContext.SaveChanges();
                                }
                                else
                                {
                                    ModelState.AddModelError(editModel.FullPropertyName(m => m.SerialNum), "Already exists");
                                }
                            }
                        }
                    }
                }
            }
            return View(UnlockedSerialPoGridList(editModel.RunName));
        }

        [HttpPost]
        [GridAction]
        public ActionResult _InsertWhtrans(BulkSaleImpItem editModel)
        {
            BulkSaleImport newImport = new BulkSaleImport();
            if (TryUpdateModel(newImport))
            {
                if (!String.IsNullOrWhiteSpace(newImport.SerialNum))
                {
                    BulkSaleImport existImport = (from i in UtilDBContext.BulkSaleImports
                                             where i.RunName == newImport.RunName && i.SerialNum == newImport.SerialNum
                                             select i).FirstOrDefault();
                    if (existImport == null)
                    {
                        UtilDBContext.BulkSaleImports.AddObject(newImport);
                        UtilDBContext.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError(editModel.FullPropertyName(m => m.SerialNum), "Already exists");
                    }
                }
                else {
                    ModelState.AddModelError(editModel.FullPropertyName(m => m.SerialNum), "Required"); 
                }
            }
            return View(UnlockedSerialPoGridList(editModel.RunName));
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteWhtrans(string id)
        {
            BulkSaleImpItem idModel = new BulkSaleImpItem();
            if (idModel.ParseItemKey(id))
            {
                    BulkSaleImport import = (from i in UtilDBContext.BulkSaleImports
                                             where i.RunName == idModel.RunName && i.SerialNum == idModel.SerialNum && i.LockedStr == "F"
                                             select i).FirstOrDefault();
                    if (import != null)
                    {
                        UtilDBContext.DeleteObject(import);
                        UtilDBContext.SaveChanges();
                    }
            }
            return View(UnlockedSerialPoGridList(idModel.RunName));
        }

        public ActionResult ExportWHEdit(string id, string columns, string orderBy, string filter)
        {
            var items = SerialPoViewModelList(id);
            return ExportExcel(items, id + "Upload", columns, orderBy, filter);
        }

        public ActionResult ExportExcel(IQueryable query, string fileName, string columns, string orderBy, string filter)
        {
            var items = query.ToGridModel(1, int.MaxValue, orderBy, string.Empty, filter).Data;

            var cols = (from c in columns.Split(';')
                       where !String.IsNullOrWhiteSpace(c)
                       let x = c.Split(':')
                       select new { 
                           prop = x[0],
                           title = x[1]
                       }).ToArray();

            //Create new Excel workbook
            var workbook = new HSSFWorkbook();

            //Create new Excel sheet
            var sheet = workbook.CreateSheet();

            //Create a header row
            var headerRow = sheet.CreateRow(0);

            for (int i = 0; i < cols.Length; i++)
            {
                sheet.SetColumnWidth(i, 12 * 256);
                headerRow.CreateCell(i).SetCellValue(cols[i].title);
            }

            sheet.CreateFreezePane(0, 1, 0, 1);

            //Populate the sheet with values from the grid data
            int rowNumber = 1;
            foreach (var item in items)
            {
                //Create a new row
                var row = sheet.CreateRow(rowNumber++);
                //Set values for the cells
                for (int i = 0; i < cols.Length; i++)
                {
                    object val = item.GetPropValue(cols[i].prop);
                    if (val != null)
                    {
                        if (val is bool)
                        {
                            row.CreateCell(i).SetCellValue((bool)val);
                        }
                        else if (val is DateTime)
                        {
                            row.CreateCell(i).SetCellValue((DateTime)val);
                        }
                        else if (val is int)
                        {
                            row.CreateCell(i).SetCellValue((int)val);
                        }
                        else if (val is double)
                        {
                            row.CreateCell(i).SetCellValue((double)val);
                        }
                        else if (val is decimal)
                        {
                            row.CreateCell(i).SetCellValue(Convert.ToDouble(val));
                        }
                        else
                        {
                            row.CreateCell(i).SetCellValue(val.ToString());
                        }
                    }
                }
            }

            //Write the workbook to a memory stream
            MemoryStream output = new MemoryStream();
            workbook.Write(output);

            //Return the result to the end user
            return File(output.ToArray(),   //The binary data of the XLS file
                "application/vnd.ms-excel", //MIME type of Excel files
                fileName + ".xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        [HttpGet]
        public ActionResult ClearWrhse(string id)
        {
            int count = CountUnLocked(id);
            if (count < 1)
            {
                throw new Exception("Warehouse " + id + " has no uploaded records!");
            }

            BulkSaleImpIModel model = new BulkSaleImpIModel();
            model.RunName = id;
            model.Count = (from i in UtilDBContext.BulkSaleImports
                           where i.RunName == id
                           select i.RunName).Count();
            return View(model);
        }

        [HttpPost]
        public ActionResult ClearWrhse(BulkSaleImpIModel model)
        {
            int count = CountUnLocked(model.IfNotNull(m => m.RunName));
            if (count < 1)
            {
                throw new Exception("Warehouse " + model.IfNotNull(m => m.RunName) + " has no uploaded records!");
            }

            EnsureUtilDBContextOpen();
            using (var transaction = UtilDBContext.Connection.BeginTransaction())
            {
                try
                {
                    // alternative is to load attachments, lots of data possibly selected just to delete, or set up cascade deletes, both in EF and DB
                    UtilDBContext.ExecuteStoreCommand("DELETE FROM FG_BULK_SALE_IMPORT WHERE FG_LOCKED = 'F' AND FG_RUN_NAME = :p0", model.RunName);
                    //UtilDBContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }
            }
            return RedirectToAction("TrnsfrWrhsList"); 
        }

        protected IQueryable<BulkSaleResult> BulkSaleResults(string run)
        {
            return from r in UtilDBContext.BulkSaleResults
                   where r.RunName == run
                   orderby r.CustPO, r.OrderNo, r.OrderLine
                   select r;
        }

        [GridAction]
        public ActionResult _SelectWhtransResults(string id)
        {
            var gridList = new GridModel(BulkSaleResults(id).ToList());
            return View(gridList);
        }

        public ActionResult ResultsWrhse(string id)
        {
            BulkSaleImpIModel model = new BulkSaleImpIModel { RunName = id };
            return View(model);
        }

        public ActionResult ExportWHResults(string id, string columns, string orderBy, string filter)
        {
            //List<BulkSaleResult> items = BulkSaleResults(id);
            var items = BulkSaleResults(id);
            return ExportExcel(items, id + "Results", columns, orderBy, filter);
        }

        [HttpGet]
        public ActionResult UploadNsPartNos()
        {
            UpldNsPartsImpIModel model = new UpldNsPartsImpIModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult UploadNsPartNos(UpldNsPartsImpIModel model, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            HttpPostedFileBase file = uploadedFiles.IfNotNull(f => f.First());
            if (file == null)
            {
                ModelState.AddModelError("", "No file was uploaded");
            }
            if (ModelState.IsValid)
            {
                model.FileName = file.FileName;
                model.QtyLoaded = 0;

                Stream stream = file.InputStream;
                StreamReader sr = new StreamReader(stream);

                int partCount = 0;
                if (model.SkipFirstRow)
                {
                    sr.ReadLine();
                }

                //PS_TO_NS_PARTNO newPart = new PS_TO_NS_PARTNO();
                //newPart.INTERNALID = "1";
                //newPart.NS_NAME = "One";
                //UtilDBContext.PS_TO_NS_PARTNOS.AddObject(newPart);
                //UtilDBContext.SaveChanges();

                        string sql = "DELETE FROM PS_TO_NS_PARTNO";
                        UtilDBContext.ExecuteStoreCommand(sql);
                        //UtilDBContext.SaveChanges();
                
                int insertCount = 0;
                insertCount = ReadFileLoadParts(sr);
                while (insertCount != 0)
	            {
                    partCount = partCount + insertCount;
	                insertCount = ReadFileLoadParts(sr);
	            }
                
                //UtilDBContext.SaveChanges();

                model.QtyLoaded = partCount;
            }
            return View(model);
        }

        private int ReadFileLoadParts(StreamReader sr)
        {
            string line;
            string[] row;
            string intId;
            string stupid;
            string[] stupids;
            string partNo;

            int insertCount = 0;

            EnsureUtilDBContextOpen();
            using (var transaction = UtilDBContext.Connection.BeginTransaction())
            {

                while ((insertCount < 1000) &&((line = sr.ReadLine()) != null))
                {
                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        row = line.Trim().Split(',');

                        if ((row != null) && (row.Length > 0))
                        {
                            intId = row.First<string>();
                            stupid = row.Last<string>();
                            if (!String.IsNullOrWhiteSpace(stupid))
                            {
                                stupids = stupid.Split(':');
                                partNo = stupids.Last<string>();
                                if (!String.IsNullOrWhiteSpace(partNo))
                                {
                                    //PS_TO_NS_PARTNO newPart = new PS_TO_NS_PARTNO();
                                    //newPart.INTERNALID = intId.Trim();
                                    //newPart.NS_NAME = partNo.Trim();
                                    //UtilDBContext.PS_TO_NS_PARTNOS.AddObject(newPart);

                                    UtilDBContext.ExecuteStoreCommand(
                                        @"INSERT INTO PS_TO_NS_PARTNO (INTERNALID, NS_NAME) VALUES (:p0, :p1)",
                                        intId.Trim().SafeSub(0, 50), partNo.Trim().SafeSub(0, 50));

                                    insertCount++;
                                }
                            }
                        }

                    }
                }
                if (insertCount > 0)
                {
                    transaction.Commit();
                }
            }
            return insertCount;
        }
    }
}

//try
//{
                                        

//}
//catch (Exception ex)
//{
//transaction.Rollback();
//ModelState.AddModelError("", "Error Saving Job: " + ex.Message);
//}

