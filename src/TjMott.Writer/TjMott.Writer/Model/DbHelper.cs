using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using TjMott.Writer.Model.Attributes;

namespace TjMott.Writer.Model
{
    public class DbHelper<T> where T : IDbType
    {
        private SQLiteConnection _connection;
        private string _tableName;

        private DbCommandHelper _selectCommand;
        private DbCommandHelper _insertCommand;
        private DbCommandHelper _deleteCommand;
        private DbCommandHelper _updateCommand;

        private SQLiteCommand _getIdCommand;
        private SQLiteCommand _selectAllIdsCommand;

        private List<PropertyInfo> _allProperties;
        private List<PropertyInfo> _subsetProperties;

        public SQLiteConnection Connection
        {
            get { return _connection; }
        }


        public DbHelper(SQLiteConnection connection)
        {
            _connection = connection;

            _tableName = ((DbTableNameAttribute)(typeof(T).GetCustomAttributes().Single(i => i is DbTableNameAttribute))).TableName;

            // Look up all properties with the DbFieldAttribute attribute defined.
            _allProperties = typeof(T).GetProperties().Where(p => p.GetCustomAttributes(typeof(DbFieldAttribute), true).Any()).ToList();
            _subsetProperties = _allProperties.Where(i => i.Name != "id").ToList(); // Doesn't include id property.

            // Create SQLite queries.
            initInsertQuery();
            initSelectQuery();
            initDeleteQuery();
            initUpdateQuery();

            _selectAllIdsCommand = new SQLiteCommand(_connection);
            _selectAllIdsCommand.CommandText = string.Format("SELECT [id] FROM {0};", _tableName);

            _getIdCommand = new SQLiteCommand(_connection);
            _getIdCommand.CommandText = "select last_insert_rowid()";
        }

        private void initInsertQuery()
        {
            _insertCommand = new DbCommandHelper(_connection);
            string fieldList = "";
            string paramList = "";

            for (int i = 0; i < _subsetProperties.Count; i++)
            {
                fieldList += "[" + _subsetProperties[i].Name + "]";
                paramList += "@" + _subsetProperties[i].Name;
                if (i < _subsetProperties.Count - 1)
                {
                    fieldList += ", ";
                    paramList += ", ";
                }
                _insertCommand.AddParameter("@" + _subsetProperties[i].Name);
            }

            string query = string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", _tableName, fieldList, paramList);
            _insertCommand.Command.CommandText = query;
        }

        private void initSelectQuery()
        {
            _selectCommand = new DbCommandHelper(_connection);
            string fieldList = "";
            for (int i = 0; i < _allProperties.Count; i++)
            {
                fieldList += "[" + _allProperties[i].Name + "]";
                if (i < _allProperties.Count - 1)
                {
                    fieldList += ", ";
                }
            }

            _selectCommand.AddParameter("@id");
            string query = string.Format("SELECT {0} FROM [{1}] WHERE id = @id", fieldList, _tableName);
            _selectCommand.Command.CommandText = query;
        }

        private void initDeleteQuery()
        {
            _deleteCommand = new DbCommandHelper(_connection);
            _deleteCommand.Command.CommandText = string.Format("DELETE FROM [{0}] WHERE [id] = @id", _tableName);
            _deleteCommand.AddParameter("@id");
        }

        private void initUpdateQuery()
        {
            _updateCommand = new DbCommandHelper(_connection);

            string paramList = "";

            for (int i = 0; i < _subsetProperties.Count; i++)
            {
                paramList += "[" + _subsetProperties[i].Name + "] = @" + _subsetProperties[i].Name;
                if (i < _subsetProperties.Count - 1)
                {
                    paramList += ", ";
                }
                _updateCommand.AddParameter("@" + _subsetProperties[i].Name);
            }

            _updateCommand.AddParameter("@id");

            string query = string.Format("UPDATE [{0}] SET {1} WHERE [id] = @id", _tableName, paramList);
            _updateCommand.Command.CommandText = query;
        }

        public List<long> GetAllIds()
        {
            List<long> retval = new List<long>();
            using (SQLiteDataReader reader = _selectAllIdsCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    long id = reader.GetInt64(0);
                    retval.Add(id);
                }
            }
            return retval;
        }

        public void Insert(T item)
        {
            for (int i = 0; i < _subsetProperties.Count; i++)
            {
                _insertCommand.Parameters["@" + _subsetProperties[i].Name].Value = _subsetProperties[i].GetValue(item);
            }
            _insertCommand.Command.ExecuteNonQuery();

            // Get new object's id.
            long id = (long)_getIdCommand.ExecuteScalar();
            item.id = id;
        }

        public void Load(T item)
        {
            _selectCommand.Parameters["@id"].Value = item.id;
            using (SQLiteDataReader reader = _selectCommand.Command.ExecuteReader())
            {
                if (reader.Read())
                {
                    for (int i = 0; i < _allProperties.Count; i++)
                    {
                        PropertyInfo p = _allProperties[i];
                        if (p.PropertyType == typeof(string))
                            p.SetValue(item, reader.GetString(i));
                        else if (p.PropertyType == typeof(bool))
                            p.SetValue(item, reader.GetBoolean(i));
                        else if (p.PropertyType == typeof(Int64))
                            p.SetValue(item, reader.GetInt64(i));
                        else if (p.PropertyType == typeof(Int32))
                            p.SetValue(item, reader.GetInt32(i));
                        else if (p.PropertyType == typeof(Int16))
                            p.SetValue(item, reader.GetInt16(i));
                        else if (p.PropertyType == typeof(byte))
                            p.SetValue(item, reader.GetByte(i));
                        else if (p.PropertyType == typeof(Nullable<Int64>))
                        {
                            object val = reader.GetValue(i);
                            if (val is DBNull)
                                p.SetValue(item, null);
                            else if (val is long)
                                p.SetValue(item, (long)val);
                        }
                        else
                            throw new Exception("Unhandled property type.");
                    }
                }
            }
        }

        public void Delete(T item)
        {
            _deleteCommand.Parameters["@id"].Value = item.id;
            _deleteCommand.Command.ExecuteNonQuery();
        }

        public void Update(T item, IEnumerable<string> propsToSave = null)
        {
            for (int i = 0; i < _allProperties.Count; i++)
            {
                if (propsToSave == null || propsToSave.Contains(_allProperties[i].Name))
                    _updateCommand.Parameters["@" + _allProperties[i].Name].Value = _allProperties[i].GetValue(item);
            }
            _updateCommand.Command.ExecuteNonQuery();
        }
    }
}
