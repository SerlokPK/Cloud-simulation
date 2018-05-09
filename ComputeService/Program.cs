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
        private static int globalIndex=-1;
        public static List<IContainer> proxies = new List<IContainer>();
        public static List<int> helpProxies = new List<int>();
        public static Dictionary<Process, string> OriginalprocessesIDs = new Dictionary<Process, string>();
        public static Dictionary<int, int> numberForProxie = new Dictionary<int, int>(); //koji proces nema pokrenut dll

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

            while (true)
            {
                CheckProcess(OriginalprocessesIDs,numberForProxie);
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
        }

        private static void CheckProcess(Dictionary<Process, string> processesIDs, Dictionary<int, int> numberForProxie)
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
                string usablePort = GetPort(processesIDs);
                int idForCalling = LastNumber(usablePort);
                
                if (FileWatcher.instances < 4)
                {
                    if(idForCalling != -1)
                    {
                        if(!CheckIfExist(numberForProxie,idForCalling))
                        {
                            helpProxies.RemoveAt(globalIndex);
                            proxies.RemoveAt(globalIndex);
                            StartSingleProcess(usablePort); //posto proveravam hasExited prop moram da izbacim iz dict i ubacim novi
                        }
                        else
                        {
                            int instance = FreeProxiesNumber(numberForProxie); //vraca indeks slobodnog 
                            proxies[instance].Load("ClientPacket.dll");
                            helpProxies.RemoveAt(globalIndex);
                            proxies.RemoveAt(globalIndex);
                            StartSingleProcess(usablePort);
                            numberForProxie.Remove(idForCalling);
                        }
                    }                 
                }else
                {
                    StartSingleProcess(usablePort);
                    helpProxies.RemoveAt(globalIndex);
                    proxies.RemoveAt(globalIndex);
                    numberForProxie.Remove(idForCalling);
                    int instance = FreeProxiesNumber(numberForProxie);
                    //proxies.RemoveAt(instance);
                    proxies[3].Load("ClientPacket.dll");
                }                     
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

        public static bool CheckIfExist(Dictionary<int, int> numberForProxie,int id)
        {
            if(numberForProxie.ContainsKey(id))
            {
                return true;
            }else
            {
                return false;
            }
        }

        public static void StartSingleProcess(string port)
        {
            Process p = Process.Start(Containers.Path.location, port);
            NetTcpBinding binding = new NetTcpBinding();
            binding.OpenTimeout = new TimeSpan(0, 10, 0);
            binding.CloseTimeout = new TimeSpan(0, 10, 0);
            binding.SendTimeout = new TimeSpan(0, 10, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
            ChannelFactory<IContainer> factory = new ChannelFactory<IContainer>(binding, new EndpointAddress($"net.tcp://localhost:{port}/IContainer")); // moraju se cuvati proxiji zbog poziva metode load n puta 
            proxies.Add(factory.CreateChannel());
            int temp = Int32.Parse(port) % 10;
            helpProxies.Add(temp);
            OriginalprocessesIDs.Add(p, port);
        }

        public static int FreeProxiesNumber(Dictionary<int, int> numberForProxie)
        {
            for(int i=0; i<4; ++i)
            {
                if(!numberForProxie.ContainsKey(i))
                {
                    numberForProxie.Add(i, i);
                    int index = helpProxies.FindIndex(x => x == i);
                    return index;
                }
            }

            return -1;
        }

        public static int LastNumber(string number)
        {
            int result=-1;
            Int32.TryParse(number, out result);

            if(result == -1)
            {
                Console.WriteLine("No valid port received.");
            }else
            {
                result = result % 10;
            }

            return result;
        }

        public static string GetPort(Dictionary<Process, string> processesIDs)
        {
            string ret = "";

            foreach(var p in processesIDs)
            {
                if(p.Key.HasExited)
                {
                    ret = p.Value;
                    processesIDs.Remove(p.Key);
                    int instance = Int32.Parse(ret) % 10;
                    globalIndex=helpProxies.FindIndex(x => x == instance);
                    

                    return ret;
                }
            }

            return ret;
        }

        
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
                    Process p= Process.Start(path, Containers.Path.ports[i]);
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.OpenTimeout = new TimeSpan(0, 10, 0);
                    binding.CloseTimeout = new TimeSpan(0, 10, 0);
                    binding.SendTimeout = new TimeSpan(0, 10, 0);
                    binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                    ChannelFactory<IContainer> factory = new ChannelFactory<IContainer>(binding, new EndpointAddress($"net.tcp://localhost:{Containers.Path.ports[i]}/IContainer")); // moraju se cuvati proxiji zbog poziva metode load n puta 
                    proxies.Add(factory.CreateChannel());
                    helpProxies.Add(i);
                    OriginalprocessesIDs.Add(p, Containers.Path.ports[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                }
            }
        }
    }
}
