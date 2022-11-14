﻿using EPiServer.Marketing.KPI.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using EPiServer.ServiceLocation;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.KPI.Manager.DataClass.Enums;
using EPiServer.Logging;

namespace EPiServer.Marketing.KPI.Manager.DataClass
{
    /// <summary>
    /// KPI object that is used to define a test characteristic(i.e. page scroll, page click, etc.)
    /// </summary>
    [DataContract]
    public class Kpi : IKpi
    {        
        public Kpi()
        {
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
        }

        /// <inheritdoc />
        [DataMember]
        public Guid Id { get; set; }

        /// <inheritdoc />
        [DataMember]
        public virtual ResultComparison ResultComparison
        {
            get
            {
                return ResultComparison.Greater;
            }
        }

        /// <summary>
        /// Overide to specify the FriendlyName to be displayed in the UI.
        /// </summary>
        [DataMember]
        public string FriendlyName {
            get {
                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
                    if( attr.text_id != null )
                    {
                        return LocalizationService.Current.GetString(attr.text_id);
                    }

                    if (attr.text == "Landing Page")
                        return "Page View";
                    else
                        return attr.text;
                }
                else
                {
                    if (GetType().Name == "Landing Page")
                        return "Page View";
                    else
                        return GetType().Name;
                }
            }
        }

        /// <summary>
        /// Indicates the result type used by the KPI.
        /// Override to properly display values in the UI.
        /// </summary>
        [DataMember]
        public virtual string KpiResultType
        {
            get
            {
                return typeof(KpiConversionResult).Name.ToString();
            }
        }

        /// <inheritdoc />
        [DataMember]
        public virtual string Description
        {
            get
            {
                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
                    if (attr.text_id != null)
                    {
                        return LocalizationService.Current.GetString(attr.description_id);
                    }
                    return attr.description;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <inheritdoc />
        [DataMember]
        public virtual string UiMarkup
        {
            get
            {
                string value;
                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
                    if (!TryGetResourceString(attr.configmarkup, out value))
                    {
                        value = LocalizationService.Current.GetString("/kpi/kpi_messaging/failed_to_load") + attr.readonlymarkup + ":" + value;
                    }
                }
                else
                {
                    value = LocalizationService.Current.GetString("/kpi/kpi_messaging/UIMarkup_not_defined");
                }
                return value;
            }
        }

        /// <inheritdoc />
        [DataMember]
        public virtual string UiReadOnlyMarkup
        {
            get
            {
                string value;
                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
                    if (!TryGetResourceString(attr.readonlymarkup, out value))
                    {
                        value = LocalizationService.Current.GetString("/kpi/kpi_messaging/failed_to_load") + attr.readonlymarkup + ":" + value;
                    }
                }
                else
                {
                    value = LocalizationService.Current.GetString("/kpi/kpi_messaging/UIMarkup_not_defined");
                }
                return value;
            }
        }

        /// <summary>
        /// Given the specified Namespace.filename key we will load the string from the file found in this assembly. If this fails 
        /// it's probably because the key is wrong or the resources is not in the assembly. See 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if loaded, else false. If false value contains the exception message.</returns>
        internal bool TryGetResourceString(string key, out string value)
        {
            bool retval = false;
            try
            {
                var assembly = this.GetType().Assembly;
                using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(key)))
                {
                    value = reader.ReadToEnd();
                }
                retval = true;
            }
            catch (Exception e)
            {
                LogManager.GetLogger().Error($"Failed to load resource {key}", e);
                value = ""; 
            }
            return retval;
        }

        /// <inheritdoc />
        [DataMember]
        public DateTime CreatedDate { get; set; }

        /// <inheritdoc />
        [DataMember]
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// KPI implentation for using the DynamicDataStore that is part of EPiServer for storing commerce related settings.
        /// </summary>
        [DataMember]
        public CommerceData PreferredCommerceFormat {get; set;}

        /// <inheritdoc />
        public virtual void Validate(Dictionary<string, string> responseData)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IKpiResult Evaluate(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Override to initalize any internal data.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Overided for any internal KPI instance cleanup.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual void Uninitialize()
        {
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public virtual event EventHandler EvaluateProxyEvent;
    }
}
