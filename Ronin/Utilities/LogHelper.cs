using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Utilities
{
    public class LogHelper
    {
        public static log4net.ILog GetLogger([CallerFilePath] string fileName = "")
        {
            var stringLength = fileName.Length;
            var lastSlashIndex = fileName.LastIndexOf("\\");
            return log4net.LogManager.GetLogger(fileName.Substring(lastSlashIndex + 1, stringLength - lastSlashIndex - 4));
        }
    }
}
