using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace UserIdentity.Services.Abstract
{
    public interface IDatabaseConnectionService : IDisposable
    {
        Task<SqlConnection> CreateConnectionAsync();
        SqlConnection CreateConnection();
    }
}