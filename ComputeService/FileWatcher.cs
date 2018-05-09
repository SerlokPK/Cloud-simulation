using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ComputeService
{
    public class FileWatcher
    {
        public static bool pairFinded = false;
        private static string path;// = @"..\..\..\Packet\";
        public static int instances = 0;
        private static List<string> usedFiles = new List<string>();

        public FileWatcher()
        {

        }
        public void watch()
        {
            
            FileSystemWatcher watcher = new FileSystemWatcher();
            path = findLocation(@"..\..\..\Location.xml");
            watcher.Path = path;
            watcher.NotifyFilter =
                                    NotifyFilters.LastWrite;
                                    
            watcher.Filter = "*.*";
            watcher.Changed += new FileSystemEventHandler(OnChanged); //mora da se kopira u bin\debug, nece drugacije
            //watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            if (!usedFiles.Contains(e.Name)) //vise puta se trigeruje event, mora ova provera
            {
                usedFiles.Add(e.Name);
                while (!IsFileReady(e.Name)) //ukoliko je file velik, moze da se desi da program nastavi, a da se nije kopiralo
                {
                }

                Console.WriteLine("New file added in folder.");
                

                if (checkPair(e.Name)) 
                {
                    string name = e.Name;

                    string subName = name.Substring(0, name.Length - 4);

                    instances = getInstance(subName + ".xml"); //validacija da moze da parsira samo xml

                    if (instances < 0 || instances > 4)
                    {
                        Console.WriteLine("Number of instances are invalid, file will be deleted.");
                        if (File.Exists(path + subName + ".xml"))
                        {
                            
                            File.Delete(path + subName + ".xml");
                        }
                            

                        if (File.Exists(path + subName + ".dll"))
                        {
                            usedFiles.Remove(e.Name);
                            File.Delete(path + subName + ".dll");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Number of instances are {instances}");

                        for(int i=0; i < instances; ++i)
                        {
                            pairFinded = true;

                            string path = Containers.Path.DDLfolder + $"Container{i + 1}\\";
                            Directory.CreateDirectory(path); //napravice, ukoliko vec ne postoji

                            if(!File.Exists(path + subName + ".dll"))
                                File.Copy(e.FullPath, path + subName + ".dll");

                            Containers.Path.currentIndex = i;
                            Program.numberForProxie.Add(i, i);
                            string ret=ComputeService.Program.proxies[i].Load(subName + ".dll"); 

                            Console.WriteLine($"Container{i} - {ret}");
                            
                        }
                    }
                }
            } 
        }

        private static string findLocation(string path)
        {
            string ret="";

            using (XmlReader reader = XmlReader.Create(path))
            {
                while (reader.Read())
                {
                    if (reader.Name == "Location" && reader.IsStartElement())
                    {
                        reader.Read();
                        try
                        {
                            ret = reader.Value;
                        }
                        catch (Exception e)
                        {
                            
                            Console.WriteLine($"Error in xml reader: {e.Message}");
                        }
                    }
                }
            }

            return ret;
        }

        private static bool checkPair(string name)
        {
            string extension = name.Substring(name.Length - 4);
            string newName=name.Substring(0,name.Length - 4);
  

            if (extension == ".xml")
            {
                if (File.Exists(path + newName + ".dll"))
                    return true;
            }else if(extension == ".dll")
            {
                if (File.Exists(path + newName + ".xml"))
                    return true;
            }

            return false;
        }

        private static bool IsFileReady(String sFilename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (inputStream.Length > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static int getInstance(string file)
        {
            int ret = -1;

            using (XmlReader reader = XmlReader.Create(path + file))
            {
                while (reader.Read())
                {
                    if (reader.Name == "Instances" && reader.IsStartElement())
                    {
                        reader.Read();
                        try
                        {
                            ret = Int32.Parse(reader.Value);
                        }
                        catch(Exception e)
                        {
                            ret = -1;
                            Console.WriteLine($"Error in xml reader: {e.Message}");
                        }
                    }
                }
            }

            return ret;
        }


    }
}
