using Leave.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBinterceptor
{

    public class DatabaseLogger : IDbCommandInterceptor
    {

        static readonly ConcurrentDictionary<DbCommand, DateTime> MStartTime = new ConcurrentDictionary<DbCommand, DateTime>();

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Log(command, interceptionContext);
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            OnStart(command);
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {

            Log(command, interceptionContext);
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {



            OnStart(command);
        }

        private static void Log<T>(DbCommand command, DbCommandInterceptionContext<T> interceptionContext)
        {

            DateTime startTime;
            TimeSpan duration;

            MStartTime.TryRemove(command, out startTime);
            if (startTime != default(DateTime))
            {
                duration = DateTime.Now - startTime;
            }
            else
                duration = TimeSpan.Zero;

            const int requestId = -1;

            var parameters = new StringBuilder();
            foreach (DbParameter param in command.Parameters)
            {
                parameters.AppendLine(param.ParameterName + " " + param.DbType + " = " + param.Value);
            }

            var message = interceptionContext.Exception == null ?
               String.Format("Database call took {0} sec. RequestId {1} \r\nCommand:\r\n{2}",
                   duration.TotalSeconds.ToString("N3"), requestId, parameters + command.CommandText)
               : String.Format(
                   "EF Database call failed after {0} sec. RequestId {1} \r\nCommand:\r\n{2}\r\nError:{3} ",
                   duration.TotalSeconds.ToString("N3"), requestId, parameters.ToString() + command.CommandText,
                   interceptionContext.Exception);

            //if (duration.TotalSeconds > 1 || message.Contains("EF Database call failed after "))
            if (message.Contains("EF Database call failed after "))
             
            {
                using (LeaveEntities dbContext = new LeaveEntities())
                {
                    error error = new error
                    {
                        TotalSeconds = (double)duration.TotalSeconds,
                        Active = true,
                        CommandType = Convert.ToString(command.CommandType),
                        CreateDate = DateTime.Now,
                        Exception = Convert.ToString(interceptionContext.Exception),
                        FileName = "",
                        InnerException = interceptionContext.Exception == null ? "" : Convert.ToString(interceptionContext.Exception.InnerException),
                        Parameters = parameters.ToString(),
                        Query = command.CommandText,
                        RequestId = 0
                    };
                    if (
                        dbContext.errors.Any(
                            a => a.Parameters == error.Parameters &&
                                a.Query == error.Query))
                        return;
                    dbContext.errors.Add(error);
                    dbContext.SaveChanges();
                }


                //var errorFileUrl = ;
                //File.WriteAllLines(, message);
            }


        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            Log(command, interceptionContext);
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {

            OnStart(command);
        }
        private static void OnStart(DbCommand command)
        {
            MStartTime.TryAdd(command, DateTime.Now);
        }


    }
}

