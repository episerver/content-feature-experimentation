﻿using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public class DalTestCriteria
    {
        public DalTestCriteria()
        {
            _filters = new List<DalABTestFilter>();
        }

        private List<DalABTestFilter> _filters;

        /// <summary>
        /// Adds the given filter to the collection of criteria filters if the property on the filter doesn't exist
        /// If the filter exists the filter will not be added
        /// </summary>
        /// <param name="filter">the filter to add</param>
        public void AddFilter(DalABTestFilter filter)
        {
            if(!_filters.Exists(f => f.Property == filter.Property))
            {
                _filters.Add(filter);
            }
        }

        public List<DalABTestFilter> GetFilters()
        {
            return _filters;
        }
    }

    public class DalABTestFilter
    {
        public DalABTestFilter(DalABTestProperty theProperty, DalFilterOperator theOperator, object theValue)
        {
            Property = theProperty;
            Operator = theOperator;
            Value = theValue;
        }

        public DalABTestFilter() { }
        
        /// <summary>
        /// The DalABTestProperty that will be filtered on
        /// </summary>
        public DalABTestProperty Property { get; set; }

        /// <summary>
        /// The operation that will be performed to filter the results set
        /// </summary>
        public DalFilterOperator Operator { get; set; }

        /// <summary>
        /// The limiter value that will be used to filter the result set
        /// </summary>
        public object Value { get; set; }
    }

    public enum DalABTestProperty
    {
        State = 0,
        OriginalItemId = 1,
        VariantId = 2
    }

    public enum DalFilterOperator
    {
        And = 0,
        Or = 1
    }

}
