using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ClientPacket
{
    public class WorkerRole : IWorkerRole
    {
        
        public void Start(string containerId)
        {
            Console.WriteLine("DLL started.");
            
        }

        public void Stop()
        {
            Console.WriteLine("DLL stopped.");
            
        }
    }
}
