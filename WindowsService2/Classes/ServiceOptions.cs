using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WindowsService2.Classes
{
    internal class ServiceOptions
    {
        public ServiceOptions()
        {
            this.ConnectionString = String.Empty;
            this.ComPortNames = new string[] { };
        }

        public ServiceOptions(string filename)
            : this()
        {
            if (File.Exists(filename))
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    XDocument doc = XDocument.Load(reader);
                    XElement root = doc.Root;
                    this.ConnectionString = (string)root
                        .Element("connection")
                        .Element("mysql");
                    this.ComPortNames = root
                        .Element("connection")
                        .Element("comports")
                        .Elements("portname")
                        .Select(x => x.Value)
                        .ToArray();
                }
            }
        }

        public string ConnectionString { get; private set; }
        public IEnumerable<string> ComPortNames { get; private set; }
    }
}
