using H2Example.Tests;
using System;
using System.Data;
using System.Data.H2;
using System.Diagnostics;

namespace H2Example
{
    class Program
    {
        public static void Main(string[] args)
        {
            InMemoryTests.RunAll();
        }
     }
}
