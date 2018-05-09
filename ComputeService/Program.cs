using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Containers;
using Services;


namespace ComputeService
{

    class Program
    {
        //public static List<string> ports = new List<string> { "10010", "10011", "10012", "10013" };
        public static List<IContainer> proxies = new List<IContainer>();
        public static Dictionary<Process, int> processesIDs = new Dictionary<Process, int>();

        static void Main(string[] args)
        {
            startProcesses();

            FileWatcher file = new FileWatcher();

            while (!FileWatcher.pairFinded)
            {
                file.watch();
                Thread.Sleep(5000);
                //Console.WriteLine("Izasao");
            }

            Console.WriteLine();
            int temp = 0;
            Process[] processList = Process.GetProcessesByName("cont1"); //originalnih 4
            foreach(Process p in processList)
            {
                processesIDs.Add(p, temp);
                ++temp;
            }

            while (true)
            {
                CheckProcess(processesIDs);
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
        }

        private static void CheckProcess(Dictionary<Process, int> processesIDs)
        {
            string status;
            if (!Exist())
            {
                 status = proxies[1].CheckState(); //odmah proverim tamo sve, zato pozivam na prvom samo
            }else
            {
                 status = proxies[0].CheckState();
            }     
           
            if (status.Equals("exit"))
            {
                Process[] newProcessList = Process.GetProcessesByName("cont1");
                int usableID= GetID(processesIDs);
                proxies[FileWatcher.instances].Load("ClientPacket.dll"); //proveri sta se desi ako ugasis onaj koji nema pokrenut dll
                
                //Process.Start(Containers.Path.location, port.ToString());                
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (true)
            {
                if (sw.ElapsedMilliseconds > 5000)
                    break;
            }

            sw.Stop();

        }

        public static int GetID(Dictionary<Process, int> processesIDs)
        {
            foreach(var p in processesIDs)
            {
                if(p.Key.HasExited)
                {
                    return p.Value;
                }
            }

            return -1;
        }

        //private static int GetID(Dictionary<int, int> processesIDs, Process[] newProcessList)
        //{
        //    int cnt=0;
        //    int ret = -1;

        //    foreach (var p in processesIDs)
        //    {
        //        if(cnt == newProcessList.Length)
        //        {
        //            ret = p.Value;
        //            break;
        //        }else
        //        {
        //            cnt = 0;
        //            for (int i = 0; i < newProcessList.Length; ++i)
        //            {
        //                if (p.Key != newProcessList[i].Id)
        //                {
        //                    cnt++;
        //                }
        //            }
        //        }
                
        //    }

        //    if(ret == -1)
        //    {
        //        ret = 4;
        //    }else
        //    {
        //        ret = ret - 1;
        //    }

        //    return ret;
        //}
        private static bool Exist()
        {
            bool ret;
            try
            {
                proxies[0].CheckState();
                ret = true;
            }catch
            {
                ret = false;
            }

            return ret;
        }
        private static void startProcesses()
        {
            string path = Containers.Path.location;

            for (int i = 0; i < 4; ++i)
            {
                try
                {
                    Process.Start(path, Containers.Path.ports[i]);
                    NetTcpBinding binding = new NetTcpBinding();
                    ChannelFactory<IContainer> factory = new ChannelFactory<IContainer>(binding, new EndpointAddress($"net.tcp://localhost:{Containers.Path.ports[i]}/IContainer")); // moraju se cuvati proxiji zbog poziva metode load n puta 
                    proxies.Add(factory.CreateChannel());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                }
            }
        }
    }
}
