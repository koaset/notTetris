﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotTetris
{
    class ErrorLogger : IDisposable
    {
        private Exception ex;
        private StreamWriter sw;
        private const string logPath = "report.log";

        public ErrorLogger(Exception ex)
        {
            this.ex = ex;
        }

        /// <summary>
        /// Saves exception info to log file
        /// </summary>
        public void Run()
        {
            using (sw = new StreamWriter(logPath, true))
            {
                sw.WriteLine(DateTime.Now.ToString());
                sw.WriteLine("Error: " + ex.ToString());
                sw.WriteLine();
            }
        }

        public void Dispose()
        {
            if (sw != null)
                sw.Dispose();
        }
    }
}
