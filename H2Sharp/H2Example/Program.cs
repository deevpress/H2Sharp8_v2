using H2Example.Tests;
using System;

namespace H2Example
{
    class Program
    {
        public static void Main(string[] args)
        {
            InMemoryTests.RunAll();

            Console.WriteLine();

            FileDatabaseTests.RunAll();
        }
     }
}
