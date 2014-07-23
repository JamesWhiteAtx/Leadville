using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;

namespace CST.Prdn.ViewModels
{
    public class FileInfoBase
    {
        public string ID { get; set; }

        public decimal DecimalID {
            get {
                if (ID != null)
                {
                    return decimal.Parse(ID);
                }
                else
                {
                    return 0;
                }
                
            }
            set { ID = value.ToString(); }
        }

        public string Description { get; set; }

        public int? AttTypeID { get; set; }

        public string AttTypeDescr { get; set; }
    }

    public class ExtantFileInfo : FileInfoBase
    {
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public string Deleted { get; set; }
        public string GroupCD { get; set; }
        public string DisplayDescr { get {
            return Description + (FileName != null ? " ("+FileName+")" : null); 
        } }
        
        public int? OrigAttTypeID { get; set; }
        public string OrigDescription { get; set; }

        public void RecordOrigVals()
        {
            OrigAttTypeID = AttTypeID;
            OrigDescription = Description;
        }

        public bool OrigValsChanged { get {
            return ((OrigAttTypeID != AttTypeID) || (OrigDescription != Description));
        } }
    }

    public class CachedFileInfo : ExtantFileInfo
    {
        public CachedFileInfo() { }

        public CachedFileInfo(string cacheID, HttpPostedFileBase upldFile, FileInfoBase src)
        {
            CacheID = cacheID;
            FileName = upldFile.FileName;
            MimeType = upldFile.ContentType;

            AttTypeID = src.IfNotNull(s => s.AttTypeID) ?? null;
            Description = src.IfNotNull(s => s.Description) ?? FileName;

            SetFileStream(upldFile.InputStream);
        }

        public string CacheID { get; set; }

        public void SetFileStream(Stream inputStream)
        {
            using (inputStream)
            {
                MemoryStream memoryStream = inputStream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    inputStream.CopyTo(memoryStream);
                }
                FileData = memoryStream.ToArray();
            }
        }

        public Byte[] FileData;
    }

    public class NewFileInfo : FileInfoBase
    {
    }

    public class DeleteFileInfo : FileInfoBase
    {
    }

    public class FileAttacher
    {
        public List<ExtantFileInfo> ViewFiles { get; set; }
        
        public List<ExtantFileInfo> ExtantFiles { get; set; }
        public int OrigExtantCount { get; set; }
        
        public List<CachedFileInfo> CachedFiles { get; set; }
        public int OrigCachedCount { get; set; }
        
        public List<NewFileInfo> NewFiles { get; set; }
        public int OrigNewCount { get; set; }
        
        public List<DeleteFileInfo> DeleteFiles { get; set; }
        public int OrigDeleteCount { get; set; }

        public List<string> DelFileIDs()
        {
            if (DeleteFiles != null)
            {
                return DeleteFiles.Select(d => d.ID).ToList();
            }
            else
            {
                return null;
            }
        }
        
        public Func<string, SelectList> AttTypesFunc { get; set; }
        
        public string Groups { get; set; }

        public int ExtantCount { get {return this.IfNotNull(a => a.ExtantFiles).IfNotNull(l => l.Count());}}
        public int CachedCount { get {return this.IfNotNull(a => a.CachedFiles).IfNotNull(l => l.Count());}}
        public int NewCount { get {return this.IfNotNull(a => a.NewFiles).IfNotNull(l => l.Count());}}
        public int DeleteCount { get { return this.IfNotNull(a => a.DeleteFiles).IfNotNull(l => l.Count()); } }

        public void RecordCounts()
        {
            OrigExtantCount = ExtantCount;
            OrigCachedCount = CachedCount;
            OrigNewCount = NewCount;
            OrigDeleteCount = DeleteCount;

            if (OrigExtantCount > 0) {
                foreach (var file in ExtantFiles)
                {
                    file.RecordOrigVals(); 
                }
            }

        }

        public bool OrigChanged
        {
            get
            {
                return ( 
                    (OrigExtantCount != ExtantCount) || (OrigCachedCount != CachedCount) || (OrigNewCount != NewCount) || (OrigDeleteCount != DeleteCount) 
                    ||
                    AnyExtantChanges
                );
            }
        }

        public bool AnyExtantChanges { get {
            if (OrigExtantCount > 0)
            {
                foreach (var file in ExtantFiles)
                {
                    if (file.OrigValsChanged)
                    {
                        return true;
                    }
                }
            }
            return false;
        } }

        private SelectList _attTypeList;

        public SelectList AttTypes() {
            if (_attTypeList == null)
            {
                _attTypeList = AttTypesFunc(Groups);
            }
            return _attTypeList;
        }
    }

    public interface IAttachedTo
    {
        FileAttacher Attacher { get; set; }
    }

}