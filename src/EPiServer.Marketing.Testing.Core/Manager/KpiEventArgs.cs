﻿using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Security;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// Used with testmanager KPI related events.
    /// </summary>
    public class KpiEventArgs : TestEventArgs
    {
        /// <summary>
        /// The KPI that the event relates to.
        /// </summary>
        public IKpi Kpi { get; private set; }

        /// <summary>
        /// Keeps track of all the KPIs for a test and used to determine when they have all converted.
        /// </summary>
        public IDictionary<Guid, bool> KpiConversionDictionary { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpi">The KPI that the event pertains to.</param>
        /// <param name="marketingTest">The test the event pertains to.</param>
        public KpiEventArgs(IKpi kpi, IMarketingTest marketingTest) : base(marketingTest)
        {
            this.Kpi = kpi;
            CurrentUser = PrincipalInfo.CurrentPrincipal.Identity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpiConversionDictionary">Keeps track of each KPI that is part of the test and whether it has converted or not.</param>
        /// <param name="marketingTest">The test the event pertains to.</param>
        public KpiEventArgs(IDictionary<Guid,bool> kpiConversionDictionary, IMarketingTest marketingTest) : base(marketingTest)
        {
            KpiConversionDictionary = kpiConversionDictionary;
        }
    }
}

