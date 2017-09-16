﻿using UserIdentity.Services.Abstract;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;

namespace UserIdentity.Services.Concrete
{
    public class DatabaseConnectionService : IDatabaseConnectionService
    {
        private SqlConnection _sqlConnection;
        private readonly string _connectionString;
        private bool disposed = false;

        public DatabaseConnectionService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<SqlConnection> CreateConnectionAsync()
        {
            _sqlConnection = new SqlConnection(_connectionString);
            await _sqlConnection.OpenAsync();

            return await Task.FromResult(_sqlConnection);
        }

        public SqlConnection CreateConnection()
        {
            _sqlConnection = new SqlConnection(_connectionString);
            _sqlConnection.Open();

            return _sqlConnection;
        }

        protected virtual void Dispose(bool disposing) {

            if (!disposing)
                return;

            if (_sqlConnection == null)
            {
                return;
            }

            _sqlConnection.Dispose();
            _sqlConnection = null;

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}