﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace raBooth.Core.Services.Printing
{
    public interface IPrintService
    {
        public void PrintImage(Mat image);
    }
}
