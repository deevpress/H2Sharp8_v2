using System.Diagnostics;
using System.Threading.Tasks;

namespace System.Data.H2
{
    /// <summary>
    /// Lightweight DbContext analog for H2 on ADO.NET .
    /// Manages connection, transaction, and simple SQL commands.
    /// </summary>
    public sealed class H2DbContext : IDisposable
    {
        private readonly H2Connection _connection;
        private H2Transaction? _transaction;

        public H2DbContext(string connectionString)
        {
            _connection = new H2Connection(connectionString);
            _connection.Open();
        }

        /// <summary>
        /// Executes SQL without return (INSERT, UPDATE, DELETE).
        /// </summary>
        public int ExecuteNonQuery(string sql)
        {
            using var cmd = new H2Command(sql, _connection, _transaction);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes SQL and returns a value (for example, COUNT(*), MAX(ID)).
        /// </summary>
        public object? ExecuteScalar(string sql)
        {
            using var cmd = new H2Command(sql, _connection, _transaction);
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// Performs a SELECT and returns a DataTable.
        /// </summary>
        public DataTable ExecuteQuery(string sql)
        {
            using var adapter = new H2DataAdapter(sql, _connection);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        /// <summary>
        /// Executes SELECT asynchronously.
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string sql)
        {
            return await Task.Run(() => ExecuteQuery(sql));
        }

        /// <summary>
        /// Starts the transaction.
        /// </summary>
        public void BeginTransaction()
        {
            _transaction = (H2Transaction)_connection.BeginTransaction();
        }

        /// <summary>
        /// Confirms the changes.
        /// </summary>
        public void Commit()
        {
            _transaction?.Commit();
            _transaction = null;
        }

        /// <summary>
        /// Rolls back the changes.
        /// </summary>
        public void Rollback()
        {
            _transaction?.Rollback();
            _transaction = null;
        }

        /// <summary>
        /// Securely releases the connection and transaction.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _transaction?.Dispose();
                _connection.Close();
                _connection.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Dispose failed: " + ex);
            }
        }
    }
}
