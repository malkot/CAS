using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using WindowsService2.Classes;

using ComParameters = System.Tuple<WindowsService2.Classes.DeviceConfig, string>;

namespace WindowsService2
{
    public partial class Service1 : ServiceBase
    {
        private ServiceConfig mConfiguration;
        private ServiceManager mServiceMan;

        public Service1()
        {
            InitializeComponent();
            this.mConfiguration = new ServiceConfig();
            this.mServiceMan = new ServiceManager();
        }

        protected override void OnStart(string[] args)
        {
            string filename = @"c:\cas-service-options.xml";

            if (args.Length == 1)
            {
                filename = args[0];
            }

            this.mConfiguration = new ServiceConfig(filename);
            this.mServiceMan.Start(this.mConfiguration);
        }

        protected override void OnStop()
        {
            this.mServiceMan.Stop();
        }
    }
}
