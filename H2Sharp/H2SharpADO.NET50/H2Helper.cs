﻿#region MIT License
/*
 * Copyright © 2008 Jonathan Mark Porter.
 * H2Sharp is a wrapper for the H2 Database Engine. http://h2sharp.googlecode.com
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion
using java.sql;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace System.Data.H2
{
    static class H2Helper
    {
		static Dictionary<int, DbType> jdbc2dbtype;
        static Dictionary<DbType, int> dbtype2jdbc;
        static Dictionary<DbType, Converter> converters2java;
        static Dictionary<int, Converter> converters2clr;
        static Dictionary<int, Type> jdbc2type;
		static void map(int jdbcType, DbType dbType, Type type, Converter converter2java, Converter converter2clr) {
			try {
				if (!jdbc2dbtype.ContainsKey(jdbcType))
					jdbc2dbtype[jdbcType] = dbType;
				
				if (!dbtype2jdbc.ContainsKey(dbType))
					dbtype2jdbc[dbType] = jdbcType;

                if (type != null && !jdbc2type.ContainsKey(jdbcType))
                    jdbc2type[jdbcType] = type;
                
                if (!converters2java.ContainsKey(dbType))
                    converters2java[dbType] = converter2java;

                if (!converters2clr.ContainsKey(jdbcType))
                    converters2clr[jdbcType] = converter2clr;
			} 
			catch (Exception) {}
		}
		static H2Helper() {
			jdbc2dbtype = new Dictionary<int, DbType>();
			dbtype2jdbc = new Dictionary<DbType, int>();
            converters2java = new Dictionary<DbType, Converter>();
            converters2clr = new Dictionary<int, Converter>();
            jdbc2type = new Dictionary<int, Type>();

            Converter id = x => x;
			map(Types.VARCHAR, DbType.AnsiString, typeof(string), id, id);
            map(Types.CHAR, DbType.AnsiStringFixedLength, typeof(string), id, id);
            map(Types.LONGVARBINARY, DbType.Binary, typeof(byte[]), 
                id, id);
            map(Types.BINARY, DbType.Binary, typeof(byte[]), id, id);

            map(Types.BLOB, DbType.Binary, typeof(byte[]),
                // CLR -> Java
                x =>
                {
                    if (x == null) return null;
                    var bytes = (byte[])x;
                    return new javax.sql.rowset.serial.SerialBlob(bytes);
                },
                // Java -> CLR
                x =>
                {
                    if (x == null) return null;
                    if (x is java.sql.Blob blob)
                    {
                        using var stream = blob.getBinaryStream();
                        using var ms = new MemoryStream();
                        var buffer = new byte[8192];
                        int read;
                        while ((read = stream.read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                        return ms.ToArray();
                    }
                    if (x is sbyte[] sbarr)
                        return sbarr.Select(b => (byte)b).ToArray();

                    return x;
                }
            );
            map(Types.BOOLEAN, DbType.Boolean, typeof(bool), 
                x => new java.lang.Boolean((bool)x),
                x => ((java.lang.Boolean)x).booleanValue()
            );

            //map(Types.TINYINT, DbType.Byte, typeof(byte), 
            //    x => new java.lang.Byte((byte)x),
            //    x => ((java.lang.Byte)x).byteValue()
            //);
            map(Types.TINYINT, DbType.Byte, typeof(byte),
                x => new java.lang.Byte((byte)x),
                x => x is java.lang.Number n ? (byte)n.intValue() : Convert.ToByte(x)
            );

            map(Types.DATE, DbType.Date, typeof(DateTime), 
                x => new java.sql.Date((long)(((DateTime)x) - UTCStart).TotalMilliseconds),
                x => UTCStart.AddMilliseconds(((java.sql.Date)x).getTime())
            );
            map(Types.TIMESTAMP, DbType.DateTime, typeof(DateTime), 
                x => new java.sql.Timestamp((long)(((DateTime)x) - UTCStart).TotalMilliseconds),
                x => UTCStart.AddMilliseconds(((java.util.Date)x).getTime())
            );
            map(Types.TIMESTAMP, DbType.DateTime2, typeof(DateTime), 
                x => new java.sql.Timestamp((long)(((DateTime)x) - UTCStart).TotalMilliseconds),
                x => UTCStart.AddMilliseconds(((java.sql.Date)x).getTime())
            );
            map(Types.TIMESTAMP, DbType.DateTimeOffset, typeof(DateTimeOffset), 
                x => new java.sql.Timestamp((long)(((DateTimeOffset)x) - UTCStart).TotalMilliseconds),
                x => (DateTimeOffset)UTCStart.AddMilliseconds(((java.sql.Date)x).getTime())
            );
            map(Types.DECIMAL, DbType.Decimal, typeof(decimal), 
                //TODO: test me !
                x => new java.math.BigDecimal(((decimal)x).ToString()),
                x => decimal.Parse(((java.math.BigDecimal)x).toString())
            );
            map(Types.DOUBLE, DbType.Double, typeof(double), 
                x => new java.lang.Double((double)x),
                x => ((java.lang.Double)x).doubleValue()
            );
            //map(Types.SMALLINT, DbType.Int16, typeof(short), 
            //    x => new java.lang.Short((short)x),
            //    x => ((java.lang.Short)x).shortValue()
            //);
            map(Types.SMALLINT, DbType.Int16, typeof(short),
                x => new java.lang.Short((short)x),
                x => x is java.lang.Number n ? (short)n.intValue() : Convert.ToInt16(x)
            );

            map(Types.INTEGER, DbType.Int32, typeof(int), 
                x => new java.lang.Integer((int)x),
                x => ((java.lang.Integer)x).intValue()
            );
            map(Types.BIGINT, DbType.Int64, typeof(long), 
                x => new java.lang.Long((long)x),
                x => ((java.lang.Long)x).longValue()
            );
            map(Types.SMALLINT, DbType.UInt16, typeof(ushort), 
                x => new java.lang.Short((short)(ushort)x),
                x => (ushort)((java.lang.Short)x).shortValue()
            );
            map(Types.INTEGER, DbType.UInt32, typeof(uint),
                x => new java.lang.Integer((int)(uint)x),
                x => (uint)((java.lang.Integer)x).intValue()
            );
            map(Types.BIGINT, DbType.UInt64, typeof(ulong), 
                x => new java.lang.Long((long)(ulong)x),
                x => (ulong)((java.lang.Long)x).longValue()
            );
            map(Types.JAVA_OBJECT, DbType.Object, typeof(Object), id, id);
            map(Types.TINYINT, DbType.SByte, typeof(byte), 
                x => new java.lang.Byte((byte)x),
                x => ((java.lang.Byte)x).byteValue()
            );
            //map(Types.FLOAT, DbType.Single, typeof(float), 
            //    x => new java.lang.Float((float)x),
            //    x => ((java.lang.Float)x).floatValue()
            //);
            map(Types.FLOAT, DbType.Single, typeof(float),
                x => new java.lang.Float((float)x),
                x => x is java.lang.Number n ? (float)n.doubleValue() : Convert.ToSingle(x)
            );
            map(Types.REAL, DbType.Single, typeof(float),
                x => new java.lang.Float((float)x),
                x => x is java.lang.Number n ? (float)n.doubleValue() : Convert.ToSingle(x)
            );


            map(Types.NVARCHAR, DbType.String, typeof(string), id, id);
            map(Types.NCHAR, DbType.StringFixedLength, typeof(string), id, id);
            map(Types.TIME, DbType.Time, typeof(DateTime), 
                x => new java.sql.Timestamp((long)(((DateTime)x) - UTCStart).TotalMilliseconds),
                x => UTCStart.AddMilliseconds(((java.sql.Date)x).getTime())
            );
            map(Types.ARRAY, DbType.VarNumeric, null, id, id);
			//DbType.Guid:
			//DbType.Currency:
		}
		public static int GetTypeCode(DbType dbType)
		{
			int ret;
			if (!dbtype2jdbc.TryGetValue(dbType, out ret))
				throw new NotSupportedException("Cannot convert the ADO.NET " + Enum.GetName(typeof(DbType), dbType) + " " + typeof(DbType).Name + " to a JDBC type");
			
			return ret;
        }
        
        public static DbType GetDbType(int typeCode)
		{
			DbType ret;
			if (!jdbc2dbtype.TryGetValue(typeCode, out ret))
				throw new NotSupportedException("Cannot convert JDBC type " + typeCode + " to an ADO.NET " + typeof(DbType).Name);
			
			return ret;
        }

        public static Converter ConverterToJava(DbType dbType)
        {
            Converter ret;
            if (!converters2java.TryGetValue(dbType, out ret))
                throw new NotSupportedException("Cannot find a converter from the ADO.NET " + Enum.GetName(typeof(DbType), dbType) + " " + typeof(DbType).Name + " to a JDBC type");

            return ret;
        }
        public static Converter ConverterToCLR(int typeCode)
        {
            Converter ret;
            if (!converters2clr.TryGetValue(typeCode, out ret))
                throw new NotSupportedException("Cannot find a converter the JDBC type " + typeCode + " to an ADO.NET " + typeof(DbType).Name);

            return ret;
        }
        public static IsolationLevel GetAdoTransactionLevel(int level)
        {
            switch (level)
            {
                case java.sql.Connection.TRANSACTION_NONE:
                    return IsolationLevel.Unspecified;
                case java.sql.Connection.TRANSACTION_READ_COMMITTED:
                    return IsolationLevel.ReadCommitted;
                case java.sql.Connection.TRANSACTION_READ_UNCOMMITTED:
                    return IsolationLevel.ReadUncommitted;
                case java.sql.Connection.TRANSACTION_REPEATABLE_READ:
                    return IsolationLevel.RepeatableRead;
                case java.sql.Connection.TRANSACTION_SERIALIZABLE:
                    return IsolationLevel.Serializable;
                default:
                    throw new NotSupportedException("unsupported transaction level");
            }
        }
        public static int GetJdbcTransactionLevel(IsolationLevel level)
        {
            switch (level)
            {
                case IsolationLevel.Unspecified:
                    return java.sql.Connection.TRANSACTION_NONE;
                case IsolationLevel.ReadCommitted:
                    return java.sql.Connection.TRANSACTION_READ_COMMITTED;
                case IsolationLevel.ReadUncommitted:
                    return java.sql.Connection.TRANSACTION_READ_UNCOMMITTED;
                case IsolationLevel.RepeatableRead:
                    return java.sql.Connection.TRANSACTION_REPEATABLE_READ;
                case IsolationLevel.Serializable:
                    return java.sql.Connection.TRANSACTION_SERIALIZABLE;
                default:
                    throw new NotSupportedException("unsupported transaction level");
            }
        }

        public delegate Object Converter(Object o);
        static readonly DateTime UTCStart = new DateTime(1970, 1, 1).ToLocalTime().AddHours(1); // TODO fix me !!!

        // TODO check this is complete
        public static Type GetType(int typeCode)
        {
            Type ret;
            if (!jdbc2type.TryGetValue(typeCode, out ret))
                throw new NotSupportedException("Cannot convert JDBC type " + typeCode + " to a CLR type");

            return ret;
        }

        public static int MapSqlTypeNameToJdbcCode(string sqlType)
        {
            switch (sqlType.ToUpperInvariant())
            {
                case "INTEGER": return 4;   // Types.INTEGER
                case "BIGINT": return -5;  // Types.BIGINT
                case "SMALLINT": return 5;   // Types.SMALLINT
                case "TINYINT": return -6;  // Types.TINYINT
                case "BOOLEAN": return 16;  // Types.BOOLEAN
                case "DOUBLE": return 8;   // Types.DOUBLE
                case "DOUBLE PRECISION": return 8;
                case "REAL": return 7;   // Types.REAL
                case "DECIMAL": return 3;   // Types.DECIMAL
                case "NUMERIC": return 2;   // Types.NUMERIC
                case "VARCHAR":
                case "CHARACTER VARYING": return 12;  // Types.VARCHAR
                case "CHAR":
                case "CHARACTER": return 1;   // Types.CHAR
                case "CLOB":
                case "CHARACTER LARGE OBJECT": return 2005;// Types.CLOB
                case "BLOB":
                case "BINARY LARGE OBJECT": return 2004;// Types.BLOB
                case "VARBINARY": return -3;  // Types.VARBINARY
                case "BINARY": return -2;  // Types.BINARY
                case "DATE": return 91;  // Types.DATE
                case "TIME": return 92;  // Types.TIME
                case "TIMESTAMP": return 93;  // Types.TIMESTAMP
                case "UUID": return 1111;// OTHER (ниже можно свести к Guid)
                case "JSON": return 1111;// OTHER
                                         // … добавь по мере встречаемости
                default: return 1111;// OTHER
            }
        }

        /*
            switch (typeCode)
            {
                case 0:
                    return typeof(DBNull);
                case -2:
                case 2004:
                case -4:
                case -3:
                    return typeof(byte[]);
                case -1:
                case 12:
                    return typeof(string);
                case 4:
                    return typeof(int);
                case 3:
                    return typeof(decimal);
                case 1:
                    return typeof(char);
                case 6:
                    return typeof(float);
                case 8:
                    return typeof(double);
                case -5:
                    return typeof(long);
                case 5:
                    return typeof(short);
                default:
                    return null;
            }
         * */
    }
}