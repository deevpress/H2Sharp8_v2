using Neolant.PFEI.SyncModule.H2.Tests;
using System;
using System.Data;
using System.Data.H2;
using System.Diagnostics;
using System.Text;

namespace H2Example.Tests
{
    public static class FileDatabaseTests
    {
        //Example of ConnectionString = "jdbc:h2:file:C:/Databases/H2_1.4.200/test_real;USER=sa;PASSWORD=h2;AUTO_SERVER=TRUE";
        private const string ConnectionString = "";

        public static void RunAll()
        {
            Console.WriteLine("Running file-based H2 tests...");
            
            if (ConnectionString == string.Empty) {
                Console.WriteLine("ConnectionString is empty, enter the correct ConnectionString.");
                return;
            }

            H2Connection conn = null;
            try
            {
                conn = new H2Connection(ConnectionString);
                conn.Open();
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteAllErrorsToConsole("Please enter the correct ConnectionString.", ex);
                return;
            }

            Console.Write("Create table");

            try { 
                new H2Command("drop table if exists FILE_TEST", conn).ExecuteNonQuery();
                new H2Command(@"
                    create table FILE_TEST(
                        ID int primary key auto_increment,
                        NAME varchar(255),
                        CREATED timestamp,
                        ACTIVE boolean,
                        AMOUNT decimal(10,2)
                    )", conn).ExecuteNonQuery();
                Console.WriteLine("\t\t\t\tOK");

                // INSERT
                Console.Write("Insert 2 rows");
                new H2Command("insert into FILE_TEST(NAME, CREATED, ACTIVE, AMOUNT) values('First', now(), true, 123.45)", conn).ExecuteNonQuery();
                new H2Command("insert into FILE_TEST(NAME, CREATED, ACTIVE, AMOUNT) values('Second', now(), false, 678.90)", conn).ExecuteNonQuery();
                Console.WriteLine("\t\t\t\tOK");

                // SELECT
                Console.Write("Select 2 rows");
                var table = new DataTable();
                new H2DataAdapter("select * from FILE_TEST order by ID", conn).Fill(table);
                Debug.Assert(table.Rows.Count == 2);
                Console.WriteLine("\t\t\t\tOK");

                // UPDATE
                Console.Write("Update row with ID = 1");
                new H2Command("update FILE_TEST set NAME='Updated', AMOUNT=999.99 where ID=1", conn).ExecuteNonQuery();
                Console.WriteLine("\t\t\tOK");

                // DELETE
                Console.Write("Delete row with ID = 1");
                new H2Command("delete from FILE_TEST where ID=2", conn).ExecuteNonQuery();
                Console.WriteLine("\t\t\tOK");

                // SELECT after modifications
                table.Clear();
                new H2DataAdapter("select * from FILE_TEST order by ID", conn).Fill(table);
                Debug.Assert(table.Rows.Count == 1);
                Console.Write($"After modifications: {table.Rows.Count} row remains");
                Console.WriteLine("\tOK");

                //  ***      BLOB tests     ***
                Console.WriteLine("Testing FILE_BLOB table...");
                Console.Write("Create table with BLOB field");
                new H2Command("drop table if exists FILE_BLOB", conn).ExecuteNonQuery();
                new H2Command(@"
                    create table FILE_BLOB(
                        ID int primary key auto_increment,
                        FILE_NAME varchar(255),
                        FILE_DATA blob
                    )", conn).ExecuteNonQuery();
                Console.WriteLine("\t\tOK");

                // Prepare fake binary data
                Console.Write("Insert BLOB-data");
                var data = Encoding.UTF8.GetBytes("Hello, H2 BLOB world!");
                var insertCmd = new H2Command("insert into FILE_BLOB(FILE_NAME, FILE_DATA) values(?, ?)", conn);
                insertCmd.Parameters.Add(new H2Parameter { Value = "test.txt" });
                insertCmd.Parameters.Add(new H2Parameter { Value = data });
                insertCmd.ExecuteNonQuery();
                Console.WriteLine("\t\t\tOK");

                // Read back
                Console.Write("Read BLOB-data");
                table = new DataTable();
                new H2DataAdapter("select * from FILE_BLOB", conn).Fill(table);

                Debug.Assert(table.Rows.Count == 1);
                var blobData = (byte[])table.Rows[0]["FILE_DATA"];
                var text = Encoding.UTF8.GetString(blobData);
                Debug.Assert(text == "Hello, H2 BLOB world!");
                Console.WriteLine("\t\t\t\tOK");

                Console.WriteLine("File-based tests OK.");

                conn.Close();
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteAllErrorsToConsole(null, ex);
            }
        }
    }
}
