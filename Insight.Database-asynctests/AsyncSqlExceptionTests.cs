namespace Insight.Database_asynctests
{
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Database;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AsyncSqlExceptionTests
    {
        private const string InvalidSql = "raiserror('Something bad happened', 16, 100);";
        private readonly ConnectionStringSettings database;

        public AsyncSqlExceptionTests()
        {
            database = ConfigurationManager.ConnectionStrings["Database"];
        }

        [TestInitialize]
        public void Initialize()
        {
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(database?.ConnectionString),
                "No connection string is configured.");
        }

        /// <summary>
        /// This test shows that the underlying dbcommand can be exectued and return the expected <code>SqlException</code>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public async Task SqlExceptionFromDbCommandAsyncThrowsSqlException()
        {
            var dbCommand = database.Connection().CreateCommand();
            dbCommand.CommandText = InvalidSql;
            dbCommand.CommandType = CommandType.Text;
            await dbCommand.Connection.OpenAsync();
            await dbCommand.ExecuteNonQueryAsync();
            dbCommand.Connection.Close();
        }

        /// <summary>
        /// This test *should* also return a <code>SqlException</code>, but instead it returns a <code>SqlException</code> wrapped
        /// in an <code>AggregateException</code>.
        /// The <code>AggregateException</code> should either not exist or it should have been unwrapped.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public async Task SqlExceptionFromExecuteSqlAsyncThrowsSqlException()
        {
            await database.Connection().ExecuteSqlAsync(InvalidSql);
        }

        /// <summary>
        /// Workaround to test for async <code>SqlException</code>s. The custom assertion catches exceptions and if it finds an
        /// <code>AggregateException</code> it unwraps it to see if the base exception is a <code>SqlException</code>.
        /// This should not be necessary.
        /// </summary>
        [TestMethod]
        public async Task SqlExceptionFromExecuteSqlAsyncThrowsSqlExceptionWithWorkaround()
        {
            await Assert.That.SqlExceptionIsThrownAsync(() => database.Connection().ExecuteSqlAsync(InvalidSql));
        }

        /// <summary>
        /// This test shows that calling <code>ExecuteSql()</code> synchronously returns the correct SqlException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void SqlExceptionFromExecuteSqlThrowsSqlException()
        {
            database.Connection().ExecuteSql(InvalidSql);
        }
    }
}