﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.DPC.PDFServer.PdfGenerator
{
    public class PdfSettings
    {
        public PageSize PageSize { get; set; }
        public float MarginTop { get; set; }
        public float MarginBottom { get; set; }
        public float MarginLeft { get; set; }
        public float MarginRight { get; set; }
    }

    public enum PageSize
    {
        A4,
        A5
    }
}
