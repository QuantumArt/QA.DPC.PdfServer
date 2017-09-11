using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services
{
    public interface IRegionTagsReplacer
    {
        Task<string> ReplaceTags(string input, int productId, int? regionId);
    }
}
