using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Services
{

    [ServiceContract]
    public interface IContainer
    {
        [OperationContract]
        String Load(String assemblyName);
        [OperationContract]
        String CheckState();
    }
}
