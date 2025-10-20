using Neolant.PFEI.SyncModule.H2.Tests;
using System;
using System.Data;
using System.Data.H2;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace H2Example.Tests
{
    public static class H2DatabaseToolsTests
    {
        private const string BaseDir = "d:/Databases/H2_2.2.222/tools_test";
        private const string DbName = "test_tools";
        private const string User = "sa";
        private const string Pass = "h2";

        public static void RunAll()
        {
            Console.WriteLine("Running H2Database tools tests...");

            try
            {
                //Ensure base directory
                if (!Directory.Exists(BaseDir))
                    Directory.CreateDirectory(BaseDir);

                var url = $"jdbc:h2:file:{BaseDir}/{DbName};USER={User};PASSWORD={Pass};AUTO_SERVER=TRUE";

                //Delete previous database files
                Console.Write("Delete old database files");
                H2Database.DeleteDbFiles(BaseDir, DbName, quiet: true);
                Console.WriteLine("\t\tOK");

                //Create a small database via RunScript
                Console.Write("Create new database via RunScript");
                var scriptFile = Path.Combine(BaseDir, "init.sql");
                var scriptText = @"
create table TEST_RUNSCRIPT(
    ID int primary key auto_increment,
    NAME varchar(64),
    CREATED timestamp default now()
);
insert into TEST_RUNSCRIPT(NAME) values('Alpha');
insert into TEST_RUNSCRIPT(NAME) values('Beta');
".Trim();

                //UTF-8 without BOM due to generation error: org.h2.jdbc.JdbcSQLSyntaxErrorException: Syntax error in SQL statement
                // "[*]\feff\000d\000a create table TEST_RUNSCRIPT(\000d\000a ID int primary key auto_increment,\000d\000a NAME ...
                var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                File.WriteAllText(scriptFile, scriptText, utf8NoBom);
                H2Database.RunScript(url, User, Pass, scriptFile, "UTF-8", continueOnError: false);
                Console.WriteLine("\tOK");

                //Backup
                Console.Write("Backup database to zip");
                var backupZip = Path.Combine(BaseDir, "backup.zip");
                H2Database.Backup(backupZip, BaseDir, DbName, quiet: true);
                Debug.Assert(File.Exists(backupZip));
                Console.WriteLine("\t\t\tOK");

                //Delete DB files
                Console.Write("Delete DB files after backup");
                H2Database.DeleteDbFiles(BaseDir, DbName, quiet: true);
                var mvFile = Path.Combine(BaseDir, $"{DbName}.mv.db");
                Debug.Assert(!File.Exists(mvFile));
                Console.WriteLine("\t\tOK");

                //Restore from zip
                Console.Write("Restore database from zip");
                H2Database.Restore(backupZip, BaseDir, DbName, quiet: true);
                Debug.Assert(File.Exists(mvFile));
                Console.WriteLine("\t\tOK");

                //Verify restored DB can open
                Console.Write("Open restored DB and count rows");
                using var conn = new H2Connection(url);
                conn.Open();
                var count = new H2Command("select count(*) from TEST_RUNSCRIPT", conn).ExecuteScalar();
                Debug.Assert(Convert.ToInt32(count) == 2);
                Console.WriteLine($"\tRows={count}\tOK");

                Console.WriteLine("H2Database tools tests OK.");
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteAllErrorsToConsole("H2Database tools tests FAIL.", ex);
            }
        }
    }
}
