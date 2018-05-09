using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Containers
{
    public class Path
    {
        public static string location = @"..\..\..\..\CloudSimulation\cont1\bin\Debug\cont1.exe";
        public static List<string> ports = new List<string> { "10010", "10011", "10012", "10013" };
        public static string DDLfolder= @"..\..\..\..\Projekat\DLLfolder\";
        public static int currentIndex = -1;
    }
}
