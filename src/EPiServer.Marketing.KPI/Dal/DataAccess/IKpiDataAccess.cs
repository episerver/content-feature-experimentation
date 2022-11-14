using System;
using System.Collections.Generic;
using System.Data.Common;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.DataAccess
{
    public interface IKpiDataAccess
    {
        DalKpi Get(Guid kpiObjectId);

        List<DalKpi> GetKpiList();

        Guid Save(DalKpi kpiObject);

        IList<Guid> Save(IList<DalKpi> kpiObjects);

        void Delete(Guid kpiObjectId);

        long GetDatabaseVersion(string schema, string contextKey);
    }
}
