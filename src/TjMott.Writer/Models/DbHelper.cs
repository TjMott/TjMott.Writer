using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Reflection;
using TjMott.Writer.Models.Attributes;
using System.Threading.Tasks;

namespace TjMott.Writer.Models
{
    public class DbHelper<T> where T : IDbType
    {
        private SqliteConnection _connection;
        private string _tableName;

        public string TableName { get => _tableName; }

        private DbCommandHelper _selectCommand;
        private DbCommandHelper _insertCommand;
        private DbCommandHelper _deleteCommand;
        private DbCommandHelper _updateCommand;

        private SqliteCommand _getIdCommand;
        private SqliteCommand _selectAllIdsCommand;

        private List<PropertyInfo> _allProperties;
        private List<PropertyInfo> _subsetProperties;

        public SqliteConnection Connection
        {
            get { return _connection; }
        }


        public DbHelper(SqliteConnection connection)
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

            _selectAllIdsCommand = new SqliteCommand(string.Format("SELECT [id] FROM {0};", _tableName), _connection);

            _getIdCommand = new SqliteCommand("select last_insert_rowid()", _connection);
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
                if (_subsetProperties[i].PropertyType == typeof(Nullable<Int64>))
                {
                    _insertCommand.Parameters["@" + _subsetProperties[i].Name].IsNullable = true;
                }
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
                if (_subsetProperties[i].PropertyType == typeof(Nullable<Int64>))
                {
                    _updateCommand.Parameters["@" + _subsetProperties[i].Name].IsNullable = true;
                }
            }

            _updateCommand.AddParameter("@id");

            string query = string.Format("UPDATE [{0}] SET {1} WHERE [id] = @id", _tableName, paramList);
            _updateCommand.Command.CommandText = query;
        }

        public List<long> GetAllIds()
        {
            return GetAllIdsAsync().Result;
        }

        public async Task<List<long>> GetAllIdsAsync()
        {
            List<long> retval = new List<long>();
            using (SqliteDataReader reader = await _selectAllIdsCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    long id = reader.GetInt64(0);
                    retval.Add(id);
                }
            }
            return retval;
        }

        public void Insert(T item)
        {
            InsertAsync(item).Wait();
        }

        public async Task InsertAsync(T item)
        {
            for (int i = 0; i < _subsetProperties.Count; i++)
            {
                if (_subsetProperties[i].GetValue(item) == null)
                {
                    _insertCommand.Parameters["@" + _subsetProperties[i].Name].Value = DBNull.Value;
                }
                else
                {
                    _insertCommand.Parameters["@" + _subsetProperties[i].Name].Value = _subsetProperties[i].GetValue(item);
                }
            }
            await _insertCommand.Command.ExecuteNonQueryAsync();

            // Get new object's id.
            long id = (long)await _getIdCommand.ExecuteScalarAsync();
            item.id = id;
        }

        public void Load(T item)
        {
            LoadAsync(item).Wait();
        }

        public async Task LoadAsync(T item)
        {
            _selectCommand.Parameters["@id"].Value = item.id;
            using (SqliteDataReader reader = await _selectCommand.Command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
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
            DeleteAsync(item).Wait();
        }

        public async Task DeleteAsync(T item)
        {
            _deleteCommand.Parameters["@id"].Value = item.id;
            await _deleteCommand.Command.ExecuteNonQueryAsync();
        }

        public void Update(T item, IEnumerable<string> propsToSave = null)
        {
            UpdateAsync(item, propsToSave).Wait();
        }

        public async Task UpdateAsync(T item, IEnumerable<string> propsToSave = null)
        {
            _updateCommand.Parameters["@id"].Value = item.id;
            for (int i = 0; i < _allProperties.Count; i++)
            {
                if (propsToSave == null || propsToSave.Contains(_allProperties[i].Name))
                {
                    if (_allProperties[i].GetValue(item) == null)
                    {
                        _updateCommand.Parameters["@" + _allProperties[i].Name].Value = DBNull.Value;
                    }
                    else
                    {
                        _updateCommand.Parameters["@" + _allProperties[i].Name].Value = _allProperties[i].GetValue(item);
                    }
                }
            }
            int affected = await _updateCommand.Command.ExecuteNonQueryAsync();
        }
    }
}
