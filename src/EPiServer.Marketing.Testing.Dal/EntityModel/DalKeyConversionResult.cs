﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public class DalKeyConversionResult : EntityBase, IDalKeyResult
    {
        public DalKeyConversionResult()
        {
            Conversions = 0;
            Weight = 1;
        }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        public Guid KpiId { get; set; }

        public int Conversions { get; set; }

        public double Weight { get; set; }

        public string SelectedWeight { get; set; }

        public int Performance { get; set; }

        public Guid? VariantId { get; set; }

        public virtual DalVariant DalVariant { get; set; }
    }
}
