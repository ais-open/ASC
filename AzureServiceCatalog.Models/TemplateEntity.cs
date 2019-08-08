using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using Wintellect.Azure.Storage.Table;

namespace AzureServiceCatalog.Models
{
    /// <summary>
    /// This implementation is based on the Wintellect Azure Storage library and uses the best practices per that library.
    /// Azure table attributes have a limit of 32KB for string attributes and 64KB for bytes. In our case, the ARM template can be greater than 32KB/64KB.
    /// Wintellect was used to handle this specific scenario and can auto-split and auto-join attributes. That's the reason this implementation for Table storage is different than the other Table implementations in the code.
    /// In case of bytes, Wintellect library can automatically split a byte array (greater than 64KB) into pre-defined number of splits.
    /// So we convert the TemplateData (which represents the ARM template) into a byte array and define the default number of splits as 5 (can be changed in appSettings)
    /// By default, this allows to handle (64KB * 5) ARM template sizes.
    /// </summary>
    public class TemplateEntity : TableEntityBase
    {
        
        public TemplateEntity(AzureTable at, DynamicTableEntity dte = null) : base(at, dte)
        {
           
        }

        public string TemplateData
        {
            get { return Get(string.Empty); }
            set
            {
                if (value != null)
                {
                    Set(value);
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] TemplateDataBytes
        {
            get {
                byte[] defaultValue = Encoding.Unicode.GetBytes("");
                return Get(defaultValue, Config.TemplateDataPropertySplitCount);
            }
            set {
                if (value != null)
                {
                    Set(value, Config.TemplateDataPropertySplitCount);
                }
            }
        }

        public string Name
        {
            get { return Get(string.Empty); }
            set
            {
                if (value != null)
                {
                    Set(value);
                }
            }
        }

        public string Description
        {
            get { return Get(string.Empty); }
            set
            {
                if (value != null)
                {
                    Set(value);
                }
            }
        }

        public string ProductImagePath
        {
            get { return Get(string.Empty); }
            set
            {
                if (value != null)
                {
                    Set(value);
                }
            }
        } //Image URI

        public double ProductPrice
        {
            get { return Get((double)0); }
            set { Set(value); }
        }

        public bool IsPublished
        {
            get { return Get(false); }
            set { Set(value); }
        }

        public static byte[] GetTemplateBytes(string templateData)
        {
            return (!string.IsNullOrEmpty(templateData)) ? Encoding.Unicode.GetBytes(templateData) : null;
        }

        public static string GetTemplateDataString(byte[] value)
        {
            return (value != null) ? Encoding.Unicode.GetString(value) : string.Empty;
        }

        public static TemplateEntity MapFromViewModel(TemplateViewModel template, AzureTable azTable)
        {
            var templateEntity = new TemplateEntity(azTable);
            templateEntity.PartitionKey = template.PartitionKey;
            templateEntity.RowKey = template.RowKey;

            templateEntity.TemplateData = string.Empty;
            templateEntity.Name = template.Name;
            templateEntity.Description = template.Description;
            templateEntity.ProductImagePath = template.ProductImagePath;
            templateEntity.ProductPrice = template.ProductPrice;
            templateEntity.IsPublished = template.IsPublished;
            templateEntity.TemplateDataBytes = GetTemplateBytes(template.TemplateData);
            return templateEntity;
        }

        public static TemplateViewModel MapToViewModel(TemplateEntity template)
        {
            var templateEntity = new TemplateViewModel();
            templateEntity.PartitionKey = template.PartitionKey;
            templateEntity.RowKey = template.RowKey;
            templateEntity.Name = template.Name;
            templateEntity.Description = template.Description;
            templateEntity.ProductImagePath = template.ProductImagePath;
            templateEntity.ProductPrice = template.ProductPrice;
            templateEntity.IsPublished = template.IsPublished;
            templateEntity.TemplateData = GetTemplateDataString(template.TemplateDataBytes);

            //Code from backward compatility. In new approach, template data is stored as bytes. 
            //But there might be old templates, where templatedata is still stored as string.
            if (string.IsNullOrEmpty(templateEntity.TemplateData))
            {
                templateEntity.TemplateData = template.TemplateData;
            }

            return templateEntity;
        }
    }
}