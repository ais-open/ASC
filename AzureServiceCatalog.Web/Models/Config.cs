using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    internal static class Config
    {
        public static readonly string ClientId = ConfigurationManager.AppSettings["ida:ClientID"];
        public static readonly string Password = ConfigurationManager.AppSettings["ida:Password"];

        public static readonly string StorageAccountEndpointSuffix = ConfigurationManager.AppSettings["ida:StorageAccountEndpointSuffix"];
        public static readonly string Authority = ConfigurationManager.AppSettings["ida:Authority"];
        public static readonly string AzureResourceManagerIdentifier = ConfigurationManager.AppSettings["ida:AzureResourceManagerIdentifier"];
        public static readonly string AzureResourceManagerUrl = ConfigurationManager.AppSettings["ida:AzureResourceManagerUrl"];
        public static readonly string AzureResourceManagerAPIVersion = ConfigurationManager.AppSettings["ida:AzureResourceManagerAPIVersion"];
        public static readonly string GraphAPIIdentifier = ConfigurationManager.AppSettings["ida:GraphAPIIdentifier"];
        public static readonly string GraphAPIVersion = ConfigurationManager.AppSettings["ida:GraphAPIVersion"];
        public static readonly string ARMAuthorizationPermissionsAPIVersion = ConfigurationManager.AppSettings["ida:ARMAuthorizationPermissionsAPIVersion"];
        public static readonly string ARMAuthorizationRoleAssignmentsAPIVersion = ConfigurationManager.AppSettings["ida:ARMAuthorizationRoleAssignmentsAPIVersion"];
        public static readonly string ARMAuthorizationRoleDefinitionsAPIVersion = ConfigurationManager.AppSettings["ida:ARMAuthorizationRoleDefinitionsAPIVersion"];
        public static readonly string AscAppId = ConfigurationManager.AppSettings["AscAppId"];

        /// <summary>
        /// When requesting data, use this hours offset for ReportEndTime from the current time.
        /// </summary>
        /// <remarks>If no offset is used, then Azure returns an error - The data requested has not yet been processed</remarks>
        public static int BillingHoursOffsetFromCurrentTimeForDataRequest
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["BillingHoursOffsetFromCurrentTimeForDataRequest"];
                if (string.IsNullOrEmpty(configValue))
                    configValue = "3";

                return int.Parse(configValue);
            }
        }

        public static string RateCardOfferId
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["RateCardOfferId"];
                if (string.IsNullOrEmpty(configValue))
                    configValue = "MS-AZR-0063P";

                return configValue;
            }
        }

        public static int TemplateDataPropertySplitCount
        {
            get
            {
                int count = 0;
                string configValue = ConfigurationManager.AppSettings["TemplateDataPropertySplitCount"];

                if (!int.TryParse(configValue, out count) || count == 0) 
                {
                    count = 5; //default value
                }

                return count;
            }
        }

        public static string NotificationFromEmailAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["NotificationFromEmailAddress"];
            }
        }

        public static string NotificationFromName
        {
            get
            {
                return ConfigurationManager.AppSettings["NotificationFromName"];
            }
        }

        public static string AdminEmailAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["AdminEmailAddress"];
            }
        }

        public static string SendGridApiKey
        {
            get
            {
                return ConfigurationManager.AppSettings["SendGridApiKey"];
            }
        }

        public static string SendGridEndPoint
        {
            get
            {
                return ConfigurationManager.AppSettings["SendGridEndPoint"];
            }
        }

        public static string FeedbackEmailSubjectFormat
        {
            get
            {
                return ConfigurationManager.AppSettings["FeedbackEmailSubjectFormat"];
            }
        }

        public static string SupportEmailSubjectFormat
        {
            get
            {
                return ConfigurationManager.AppSettings["SupportEmailSubjectFormat"];
            }
        }

        public static string DefaultAdGroup
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["DefaultAdGroup"];
                if (string.IsNullOrEmpty(configValue))
                    configValue = "ASCGrouping";

                return configValue;
            }
        }

        public static string DefaultResourceGroup
        {
            get
            {
                string configValue = ConfigurationManager.AppSettings["DefaultResourceGroup"];
                if (string.IsNullOrEmpty(configValue))
                    configValue = "ASCRG";

                return configValue;
            }
        }
    }
}