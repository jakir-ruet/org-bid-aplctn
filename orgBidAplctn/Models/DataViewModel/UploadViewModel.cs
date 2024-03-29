using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace orgBidAplctn.Models.DataViewModel
{
    public class UploadViewModel
    {
        public IFormFile ExcelFile { get; set; }
    }
}
