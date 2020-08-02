using System;

namespace TjMott.Writer.Model.Attributes
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
