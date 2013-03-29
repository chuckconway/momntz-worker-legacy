using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Momntz.Worker.Core;

namespace Momntz.Worker
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            QueueService service = new QueueService();
            service.Process();
        }

        protected override void OnStop()
        {
        }
    }
}
