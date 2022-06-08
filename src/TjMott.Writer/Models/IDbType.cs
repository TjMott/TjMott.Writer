using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace TjMott.Writer.Models
{
    public interface IDbType
    {
        long id { get; set; }
        SqliteConnection Connection { get; set; }
        Task LoadAsync();
        Task CreateAsync();
        Task SaveAsync();
        Task DeleteAsync();
    }
}
