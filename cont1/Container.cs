using Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace cont1
{
    
    public class Container : IContainer
    {
        public string Load(string assemblyName)
        {
            string assembly = Path.GetFullPath(assemblyName);
            
            var DLL = Assembly.LoadFile(assembly);

            var theType = DLL.GetType("ClientPacket.WorkerRole");
            var c = Activator.CreateInstance(theType);
            var method = theType.GetMethod("Start");
            method.Invoke(c, new object[] { "1"});  

            return "ok";
        }

        public string CheckState()
        {
            Process[] processlist = Process.GetProcessesByName("cont1"); //izvuce pozvane cont1 procese
            //list<int> processid = new list<int>();
            string ret = "";

            if (processlist.Length < 4)
            {
                ret = "exit";
            }
            else
            {
                ret = "ok";
            }


            return ret;
        }
    }
}
