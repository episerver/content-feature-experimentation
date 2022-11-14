﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public class DalVariant : EntityBase
    {
        public DalVariant()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        /// <summary>
        /// Id of the test this is associated with.
        /// </summary>
        public Guid? TestId { get; set; }

        /// <summary>
        /// Id of a variant to use instead of the original item for a test.
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Version of original item that is selected as a variant.
        /// </summary>
        public int ItemVersion { get; set; }

        /// <summary>
        /// True if variant won the test, false otherwise.
        /// </summary>
        public bool IsWinner { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Conversions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Views { get; set; }

        /// <summary>
        /// Marks the variant content as the one that is publshed i.e. not the draft that is part of the test.
        /// </summary>
        public bool IsPublished { get; set; }
        
        /// <summary>
        /// Reference to the test this is associated with.
        /// </summary>
        public virtual DalABTest DalABTest { get; set; }

        public virtual IList<DalKeyFinancialResult> DalKeyFinancialResults { get; set; } = new List<DalKeyFinancialResult>();

        public virtual IList<DalKeyValueResult> DalKeyValueResults { get; set; } = new List<DalKeyValueResult>();

        public virtual IList<DalKeyConversionResult> DalKeyConversionResults { get; set; } = new List<DalKeyConversionResult>();
    }
}
