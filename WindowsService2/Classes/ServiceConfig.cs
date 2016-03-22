using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WindowsService2.Classes
{
    internal class ServiceConfig
    {
        public ServiceConfig()
        {
            this.SqlConnectionString = String.Empty;
            this.Devices = new DeviceConfig[] { };
        }

        public ServiceConfig(string filename)
            : this()
        {
            if (File.Exists(filename))
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    XDocument doc = XDocument.Load(reader);
                    XElement root = doc.Root;
                    this.SqlConnectionString = (string)root
                        .Element("connection")
                        .Element("mysql");
                    this.Devices = root
                        .Element("connection")
                        .Element("devices")
                        .Elements("device")
                        .Select(x => new DeviceConfig()
                            {
                                Id = (int)x.Element("id"),
                                PortName = (string)x.Element("port")
                            })
                        .ToArray();
                }
            }
        }

        public string SqlConnectionString { get; private set; }
        public IEnumerable<DeviceConfig> Devices { get; private set; }
    }
}
