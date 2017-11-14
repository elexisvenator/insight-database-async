namespace Insight.Database_asynctests
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class AssertExtensions
    {
        public static async Task SqlExceptionIsThrownAsync(this Assert assert, Func<Task> action)
        {
            try
            {
                await action();
                Assert.Fail("Expected SqlException but no exception was thrown.");
            }
            catch (SqlException)
            {
                //Expected, all good.
            }
            catch (AggregateException e)
            {
                // unwrap the aggregate exception
                var baseException = e.GetBaseException();

                if (baseException is SqlException)
                {
                    //Expected, all good.
                    return;
                }

                Assert.Fail(
                    $"Expected SqlException but {baseException.GetType().Name} was thrown.{Environment.NewLine}Error: {baseException}");
            }
        }
     }
}