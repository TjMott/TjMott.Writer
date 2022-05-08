using System;

namespace TjMott.Writer.Models.Attributes
{
    public class DbTableNameAttribute : System.Attribute
    {
        public string TableName { get; set; }

        public DbTableNameAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}
