using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CST.Prdn.ViewModels
{
    public class IPEndPointModel
    {
        [Display(Name = "Address")]
        [Required]
        public string HostName { get; set; }

        [Display(Name = "Port")]
        [Required]
        public int Port { get; set; }
    }

    public class IPEndPointTestModel : IPEndPointModel
    {
        [Display(Name = "Message")]
        public string Message { get; set; }
    }
}