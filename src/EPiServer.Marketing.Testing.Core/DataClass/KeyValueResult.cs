﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    /// <summary>
    /// KPI result that handles any numerical value.
    /// </summary>
    public class KeyValueResult : CoreEntityBase, IKeyResult
    {
        /// <inheritdoc />
        public Guid KpiId { get; set; }

        /// <summary>
        /// Numerical value to bs saved.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// ID of the variant the result pertains to.
        /// </summary>
        public Guid? VariantId { get; set; }

        /// <summary>
        /// The variant the result pertains to.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual Variant Variant { get; set; }
    }
}
