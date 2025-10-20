using Neolant.PFEI.SyncModule.H2.Tests;
using System;
using System.Data;
using System.Data.H2;
using System.Diagnostics;

namespace H2Example.Tests
{
    public static class H2DbContextTests
    {
        private const string ConnectionString =
            "jdbc:h2:file:d:/Databases/H2_2.2.222/test_context;USER=sa;PASSWORD=h2;AUTO_SERVER=TRUE";

        public static void RunAll()
        {
            Console.WriteLine("Running H2DbContext tests...");

            try
            {
                using var ctx = new H2DbContext(ConnectionString);
                ctx.Open();

                Console.Write("Connection open");
                Debug.Assert(ctx.Connection.State == ConnectionState.Open);
                Console.WriteLine("\t\t\t\tOK");

                Console.Write("Create table CTX_SAMPLE");
                ctx.ExecuteNonQuery("drop table if exists CTX_SAMPLE");
                ctx.ExecuteNonQuery(@"
                    create table CTX_SAMPLE(
                        ID int primary key auto_increment,
                        NAME varchar(128),
                        CREATED timestamp default now()
                    )");
                Console.WriteLine("\t\t\tOK");

                Console.Write("Insert 2 rows");
                ctx.ExecuteNonQuery("insert into CTX_SAMPLE(NAME) values('First')");
                ctx.ExecuteNonQuery("insert into CTX_SAMPLE(NAME) values('Second')");
                Console.WriteLine("\t\t\t\tOK");

                Console.Write("Count rows (ExecuteScalar)");
                var count = Convert.ToInt32(ctx.ExecuteScalar("select count(*) from CTX_SAMPLE"));
                Debug.Assert(count == 2);
                Console.WriteLine($"\t\tOK(Count={count})");

                Console.Write("Query all rows");
                var table = ctx.ExecuteQuery("select * from CTX_SAMPLE order by ID");
                Debug.Assert(table.Rows.Count == 2);
                Console.WriteLine("\t\t\t\tOK");

                Console.Write("Begin/Rollback transaction test");
                ctx.BeginTransaction();
                ctx.ExecuteNonQuery("insert into CTX_SAMPLE(NAME) values('InTx')");
                ctx.Rollback();
                var count2 = Convert.ToInt32(ctx.ExecuteScalar("select count(*) from CTX_SAMPLE"));
                Debug.Assert(count2 == 2);
                Console.WriteLine("\t\tOK");

                Console.Write("Commit transaction test");
                ctx.BeginTransaction();
                ctx.ExecuteNonQuery("insert into CTX_SAMPLE(NAME) values('Committed')");
                ctx.Commit();
                var count3 = Convert.ToInt32(ctx.ExecuteScalar("select count(*) from CTX_SAMPLE"));
                Debug.Assert(count3 == 3);
                Console.WriteLine("\t\t\tOK");

                Console.WriteLine("H2DbContext tests OK.");
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteAllErrorsToConsole("H2DbContextTests FAIL", ex);
            }
        }
    }
}
