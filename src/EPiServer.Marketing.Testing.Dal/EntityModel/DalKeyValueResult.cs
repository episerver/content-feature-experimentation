using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public class DalKeyValueResult : EntityBase, IDalKeyResult
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        public Guid KpiId { get; set; }

        public double Value { get; set; }

        public Guid? VariantId { get; set; }

        public virtual DalVariant DalVariant { get; set; }
    }
}
