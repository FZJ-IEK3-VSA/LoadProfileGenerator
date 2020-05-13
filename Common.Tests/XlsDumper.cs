using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Automation.ResultFiles;
using JetBrains.Annotations;
using NUnit.Framework;
using OfficeOpenXml;

namespace Common.Tests
{
    public class Row
    {
        public Row([NotNull] [ItemNotNull] List<RowValue> values)
        {
            Values = values;
        }

        [NotNull]
        [ItemNotNull]
        public List<RowValue> Values { get; }

    }
    [AttributeUsage(AttributeTargets.Property)]

    public class RowBuilderIgnoreAttribute : Attribute
    {

    }

    public enum ValueOrEquation
    {
        Value,
        Equation
    }
    public class RowValue
    {
        [NotNull]
        public string Name { get; }
        [CanBeNull]
        public object Value { get; set; }

        public RowValue([NotNull] string name, [CanBeNull] object value, ValueOrEquation valueOrEquation = ValueOrEquation.Value)
        {
            Name = name;
            Value = value;
            ValueOrEquation = valueOrEquation;
        }

        public ValueOrEquation ValueOrEquation { get; }
    }
    public class XlsRowBuilder
    {
        [NotNull]
        [ItemNotNull]
        public List<RowValue> RowValues { get; } = new List<RowValue>();

        [NotNull]
        public XlsRowBuilder Add([NotNull] string name, [CanBeNull] object content)
        {
            if (RowValues.Any(x => x.Name == name))
            {
                throw new LPGException("Key was already added for this row: " + name);
            }

            RowValues.Add(new RowValue(name, content));
            return this;
        }

        [NotNull]
        public XlsRowBuilder AddToPossiblyExisting([NotNull] string name, double content)
        {
            var other = RowValues.FirstOrDefault(x => x.Name == name);
            if (other != null)
            {
                if (other.Value == null)
                {
                    throw new LPGException("was null");
                }
                other.Value = (double)other.Value + content;
            }
            else
            {
                RowValues.Add(new RowValue(name, content));
            }

            return this;
        }

        [NotNull]
        public static XlsRowBuilder GetAllProperties([NotNull] object o)
        {
            var rb = new XlsRowBuilder();
            var properties = o.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var shouldIgnoreAttribute = Attribute.IsDefined(property, typeof(RowBuilderIgnoreAttribute));
                if (shouldIgnoreAttribute)
                {
                    continue;
                }

                object val = property.GetValue(o);
                if (o is List<string> mylist)
                {
                    val = string.Join(",", mylist);
                }

                rb.Add(property.Name, val);
            }

            return rb;
        }

        [NotNull]
        public Row GetRow() => new Row(RowValues);

        public void Merge([NotNull] XlsRowBuilder toRowBuilder)
        {
            foreach (var line in toRowBuilder.RowValues)
            {
                RowValues.Add(new RowValue(line.Name, line.Value));
            }
        }

        [NotNull]
        public static XlsRowBuilder Start([NotNull] string name, [CanBeNull] object content)
        {
            var rb = new XlsRowBuilder();
            return rb.Add(name, content);
        }
    }
    public class RowCollection
    {
        public RowCollection([NotNull] string sheetName)
        {
            if (sheetName.Length > 30)
            {
                throw new LPGException("RowLength > 30 chars, not allowed: " + sheetName + " was " + sheetName.Length);
            }
            SheetName = sheetName;
        }

        [NotNull]
        public string SheetName { get; }

        [NotNull]
        [ItemNotNull]
        public List<Row> Rows { get; } = new List<Row>();

        [NotNull]
        [ItemNotNull]
        public List<string> ColumnsToSum { get; } = new List<string>();
        public double SumDivisionFactor { get; set; } = 1;

        public void Add([NotNull] XlsRowBuilder rowBuilder)
        {
            Rows.Add(rowBuilder.GetRow());
        }

        [NotNull]
        public static RowCollection MakeRowCollectionFromObjects<T>([NotNull] List<T> objects, [NotNull] string name)
        {
            RowCollection rc = new RowCollection(name);
            foreach (var o in objects) {
                var rbc =  XlsRowBuilder.GetAllProperties(o);
                rc.Add(rbc);
            }
            return rc;
        }
    }
    public class XlsxDumperTest
        {

            public class Myclass {
                public int MyVal1 { get; } = 1;
                public string MyVal2 { get; } = "blub";
            }
            [Test]
            public void RunTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                RowCollection rc = new RowCollection("mysheet");
                var rb = XlsRowBuilder.Start("t1", "mytxt").Add("blub", 1).Add("blub2", 1);
                rc.Add(rb);
                var rb2 = XlsRowBuilder.Start("t1", "mytxt").Add("blub", 1).Add("blub5", 1);
                rc.Add(rb2);
                Myclass mc = new Myclass();
                RowCollection rc2 = new RowCollection("sheet2");
                var rbc = XlsRowBuilder.GetAllProperties(mc);
                rc2.Add(rbc);
                XlsxDumper.WriteToXlsx(wd.Combine("t.xlsx"), rc, rc2);
            }
        }
    }

        public static class XlsxDumper
        {

            [NotNull]
            public static string GetExcelColumnName(int columnNumber)
            {
                int dividend = columnNumber;
                string columnName = string.Empty;

                while (dividend > 0)
                {
                    var modulo = (dividend - 1) % 26;
                    columnName = Convert.ToChar(65 + modulo) + columnName;
                    dividend = (dividend - modulo) / 26;
                }

                return columnName;
            }


            public static void WriteToXlsx([NotNull] string fileName, [NotNull] [ItemNotNull] List<RowCollection> rowCollections)
            {
                var p = PrepareExcelPackage(fileName);

                foreach (var rowCollection in rowCollections)
                {
                    var ws = p.Workbook.Worksheets.Add(rowCollection.SheetName);
                    FillExcelSheet(rowCollection, ws);
                }

                p.Save();
                p.Dispose();
            }

            public static void WriteToXlsx([NotNull] string fileName, [NotNull] [ItemNotNull] params RowCollection[] rowCollections)
            {
                var p = PrepareExcelPackage(fileName);
                foreach (var rowCollection in rowCollections)
                {
                    var ws = p.Workbook.Worksheets.Add(rowCollection.SheetName);
                    FillExcelSheet(rowCollection, ws);
                }

                p.Save();
                p.Dispose();
            }



            private static void FillExcelSheet([NotNull] RowCollection rc, [NotNull] ExcelWorksheet ws, bool addFilter = true, int rowoffset = 1)
            {
                if (rc.Rows.Count == 0)
                {
                    throw new LPGException("Not a single row to export. This is probably not intended.");
                }

                if (string.IsNullOrWhiteSpace(rc.SheetName))
                {
                    throw new LPGException("Sheetname was null");
                }

                List<string> keys = rc.Rows.SelectMany(x => x.Values.Select(y => y.Name)).Distinct().ToList();
                Dictionary<string, int> colidxByKey = new Dictionary<string, int>();
                Dictionary<string, int> colsToSum = new Dictionary<string, int>();
                for (int i = 0; i < keys.Count; i++)
                {
                    colidxByKey.Add(keys[i], i + 1);
                    ws.Cells[rowoffset, i + 1].Value = keys[i];
                    if (rc.ColumnsToSum.Contains(keys[i]))
                    {
                        colsToSum.Add(keys[i], i + 1);
                    }
                }

                int rowIdx = 1 + rowoffset;
                foreach (Row row in rc.Rows)
                {
                    foreach (var pair in row.Values)
                    {
                        int col = colidxByKey[pair.Name];
                        ws.Cells[rowIdx, col].Value = pair.Value;
                    }

                    rowIdx++;
                }

                int colOffSet = colidxByKey.Count + 2;
                rowIdx = 2;
                foreach (var pair in colsToSum)
                {
                    string col = GetExcelColumnName(pair.Value);
                    ws.Cells[rowIdx, colOffSet].Value = pair.Key;
                    ws.Cells[rowIdx, colOffSet + 1].Formula = "=sum(" + col + ":" + col + ")/" + rc.SumDivisionFactor;
                    rowIdx++;
                }

                ws.View.FreezePanes(2, 1);
                string lastCol = GetExcelColumnName(colidxByKey.Count);
                if (addFilter)
                {
                    ws.Cells["A1:" + lastCol + "1"].AutoFilter = true;
                }
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
        }



            [NotNull]
            private static ExcelPackage PrepareExcelPackage([NotNull] string fileName)
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    Thread.Sleep(250);
                }

                var p = new ExcelPackage(new FileInfo(fileName));
                return p;
            }
        }
}
