﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Marketing.KPI.Manager.DataClass;

namespace EPiServer.Marketing.Testing.Web.Models
{
    public class KpiTypeModel
    {
        public IKpi kpi { get; set; }
        public string kpiType { get; set; }
    }
}
