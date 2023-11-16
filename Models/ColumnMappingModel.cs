using System;
namespace NoahWeb_Private_Asset_Module.Models
{
    public class ColumnMappingModel
    {
        public string FileName { get; set; }  // The name of the uploaded CSV file (to re-process it)
        public Dictionary<string, string> Mappings { get; set; }  // Mapping between CSV columns and Transaction model properties
    }

}

