﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PowerEventProviderService
{
    public partial class Service : ServiceBase
    {


        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            
        }

        

        protected override void OnStop()
        {
        }
    }
}
