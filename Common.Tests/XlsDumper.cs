//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using Automation;
//using Automation.ResultFiles;
//using JetBrains.Annotations;
////using OfficeOpenXml;
//using Xunit;
//using Xunit.Abstractions;

//namespace Common.Tests
//{
//    public class Row
//    {
//        public Row([JetBrains.Annotations.NotNull] [ItemNotNull] List<RowValue> values)
//        {
//            Values = values;
//        }

//        [JetBrains.Annotations.NotNull]
//        [ItemNotNull]
//        public List<RowValue> Values { get; }

//    }
//    [AttributeUsage(AttributeTargets.Property)]

//    public class RowBuilderIgnoreAttribute : Attribute
//    {

//    }

//    public enum ValueOrEquation
//    {
//        Value,
//        Equation
//    }
//    public class RowValue
//    {
//        [JetBrains.Annotations.NotNull]
//        public string Name { get; }
//        public object? Value { get; set; }

//        public RowValue([JetBrains.Annotations.NotNull] string name, object? value, ValueOrEquation valueOrEquation = ValueOrEquation.Value)
//        {
//            Name = name;
//            Value = value;
//            ValueOrEquation = valueOrEquation;
//        }

//        public ValueOrEquation ValueOrEquation { get; }
//    }
//    public class XlsRowBuilder
//    {
//        [JetBrains.Annotations.NotNull]
//        [ItemNotNull]
//        public List<RowValue> RowValues { get; } = new List<RowValue>();

//        [JetBrains.Annotations.NotNull]
//        public XlsRowBuilder Add([JetBrains.Annotations.NotNull] string name, object? content)
//        {
//            if (RowValues.Any(x => x.Name == name))
//            {
//                throw new LPGException("Key was already added for this row: " + name);
//            }

//            RowValues.Add(new RowValue(name, content));
//            return this;
//        }

//        [JetBrains.Annotations.NotNull]
//        public XlsRowBuilder AddToPossiblyExisting([JetBrains.Annotations.NotNull] string name, double content)
//        {
//            var other = RowValues.FirstOrDefault(x => x.Name == name);
//            if (other != null)
//            {
//                if (other.Value == null)
//                {
//                    throw new LPGException("was null");
//                }
//                other.Value = (double)other.Value + content;
//            }
//            else
//            {
//                RowValues.Add(new RowValue(name, content));
//            }

//            return this;
//        }

//        [JetBrains.Annotations.NotNull]
//        public static XlsRowBuilder GetAllProperties(object? o)
//        {
//            var rb = new XlsRowBuilder();
//            if (o == null) {
//                throw new LPGException("object was null");
//            }
//            var properties = o.GetType().GetProperties();
//            foreach (PropertyInfo property in properties)
//            {
//                var shouldIgnoreAttribute = Attribute.IsDefined(property, typeof(RowBuilderIgnoreAttribute));
//                if (shouldIgnoreAttribute)
//                {
//                    continue;
//                }

//                object? val = property.GetValue(o);
//                if (o is List<string> mylist)
//                {
//                    val = string.Join(",", mylist);
//                }

//                rb.Add(property.Name, val);
//            }

//            return rb;
//        }

//        [JetBrains.Annotations.NotNull]
//        public Row GetRow() => new Row(RowValues);

//        public void Merge([JetBrains.Annotations.NotNull] XlsRowBuilder toRowBuilder)
//        {
//            foreach (var line in toRowBuilder.RowValues)
//            {
//                RowValues.Add(new RowValue(line.Name, line.Value));
//            }
//        }

//        [JetBrains.Annotations.NotNull]
//        public static XlsRowBuilder Start([JetBrains.Annotations.NotNull] string name, object? content)
//        {
//            var rb = new XlsRowBuilder();
//            return rb.Add(name, content);
//        }
//    }
//    public class RowCollection
//    {
//        public RowCollection([JetBrains.Annotations.NotNull] string sheetName)
//        {
//            if (sheetName.Length > 30)
//            {
//                throw new LPGException("RowLength > 30 chars, not allowed: " + sheetName + " was " + sheetName.Length);
//            }
//            SheetName = sheetName;
//        }

//        [JetBrains.Annotations.NotNull]
//        public string SheetName { get; }

//        [JetBrains.Annotations.NotNull]
//        [ItemNotNull]
//        public List<Row> Rows { get; } = new List<Row>();

//        [JetBrains.Annotations.NotNull]
//        [ItemNotNull]
//        public List<string> ColumnsToSum { get; } = new List<string>();
//        public double SumDivisionFactor { get; set; } = 1;

//        public void Add([JetBrains.Annotations.NotNull] XlsRowBuilder rowBuilder)
//        {
//            Rows.Add(rowBuilder.GetRow());
//        }

//        [JetBrains.Annotations.NotNull]
//        public static RowCollection MakeRowCollectionFromObjects<T>([JetBrains.Annotations.NotNull] List<T> objects, [JetBrains.Annotations.NotNull] string name)
//        {
//            RowCollection rc = new RowCollection(name);
//            foreach (var o in objects) {
//                var rbc =  XlsRowBuilder.GetAllProperties(o);
//                rc.Add(rbc);
//            }
//            return rc;
//        }
//    }
////    public class XlsxDumperTest:UnitTestBaseClass
////        {

////            public class MyTst {
////                public int MyVal1 { get; } = 1;
////                public string MyVal2 { get; } = "blub";
////            }
////            [Fact]
////            [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
////        public void RunXlsDumperBasicTest()
////        {
////            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
////            {
////                RowCollection rc = new RowCollection("mysheet");
////                var rb = XlsRowBuilder.Start("t1", "mytxt").Add("blub", 1).Add("blub2", 1);
////                rc.Add(rb);
////                var rb2 = XlsRowBuilder.Start("t1", "mytxt").Add("blub", 1).Add("blub5", 1);
////                rc.Add(rb2);
////                MyTst mc = new MyTst();
////                RowCollection rc2 = new RowCollection("sheet2");
////                var rbc = XlsRowBuilder.GetAllProperties(mc);
////                rc2.Add(rbc);
////                XlsxDumper.WriteToXlsx(wd.Combine("t.xlsx"), rc, rc2);
////            }
////        }

////        public XlsxDumperTest([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
////        {
////        }
////        }

////        public static class XlsxDumper
////        {

////            [JetBrains.Annotations.NotNull]
////            public static string GetExcelColumnName(int columnNumber)
////            {
////                int dividend = columnNumber;
////                string columnName = string.Empty;

////                while (dividend > 0)
////                {
////                    var modulo = (dividend - 1) % 26;
////                    columnName = Convert.ToChar(65 + modulo) + columnName;
////                    dividend = (dividend - modulo) / 26;
////                }

////                return columnName;
////            }


////            //public static void WriteToXlsx([JetBrains.Annotations.NotNull] string fileName, [JetBrains.Annotations.NotNull] [ItemNotNull] List<RowCollection> rowCollections)
////            //{
////            //    var p = PrepareExcelPackage(fileName);

////            //    foreach (var rowCollection in rowCollections)
////            //    {
////            //        var ws = p.Workbook.Worksheets.Add(rowCollection.SheetName);
////            //        FillExcelSheet(rowCollection, ws);
////            //    }

////            //    p.Save();
////            //    p.Dispose();
////            //}

////            //public static void WriteToXlsx([JetBrains.Annotations.NotNull] string fileName, [JetBrains.Annotations.NotNull] [ItemNotNull] params RowCollection[] rowCollections)
////            //{
////            //    var p = PrepareExcelPackage(fileName);
////            //    foreach (var rowCollection in rowCollections)
////            //    {
////            //        var ws = p.Workbook.Worksheets.Add(rowCollection.SheetName);
////            //        FillExcelSheet(rowCollection, ws);
////            //    }

////            //    p.Save();
////            //    p.Dispose();
////            //}



////        //    private static void FillExcelSheet([JetBrains.Annotations.NotNull] RowCollection rc, [JetBrains.Annotations.NotNull] ExcelWorksheet ws, bool addFilter = true, int rowoffset = 1)
////        //    {
////        //        if (rc.Rows.Count == 0)
////        //        {
////        //            throw new LPGException("Not a single row to export. This is probably not intended.");
////        //        }

////        //        if (string.IsNullOrWhiteSpace(rc.SheetName))
////        //        {
////        //            throw new LPGException("Sheetname was null");
////        //        }

////        //        List<string> keys = rc.Rows.SelectMany(x => x.Values.Select(y => y.Name)).Distinct().ToList();
////        //        Dictionary<string, int> colidxByKey = new Dictionary<string, int>();
////        //        Dictionary<string, int> colsToSum = new Dictionary<string, int>();
////        //        for (int i = 0; i < keys.Count; i++)
////        //        {
////        //            colidxByKey.Add(keys[i], i + 1);
////        //            ws.Cells[rowoffset, i + 1].Value = keys[i];
////        //            if (rc.ColumnsToSum.Contains(keys[i]))
////        //            {
////        //                colsToSum.Add(keys[i], i + 1);
////        //            }
////        //        }

////        //        int rowIdx = 1 + rowoffset;
////        //        foreach (Row row in rc.Rows)
////        //        {
////        //            foreach (var pair in row.Values)
////        //            {
////        //                int col = colidxByKey[pair.Name];
////        //                ws.Cells[rowIdx, col].Value = pair.Value;
////        //            }

////        //            rowIdx++;
////        //        }

////        //        int colOffSet = colidxByKey.Count + 2;
////        //        rowIdx = 2;
////        //        foreach (var pair in colsToSum)
////        //        {
////        //            string col = GetExcelColumnName(pair.Value);
////        //            ws.Cells[rowIdx, colOffSet].Value = pair.Key;
////        //            ws.Cells[rowIdx, colOffSet + 1].Formula = "=sum(" + col + ":" + col + ")/" + rc.SumDivisionFactor;
////        //            rowIdx++;
////        //        }

////        //        ws.View.FreezePanes(2, 1);
////        //        string lastCol = GetExcelColumnName(colidxByKey.Count);
////        //        if (addFilter)
////        //        {
////        //            ws.Cells["A1:" + lastCol + "1"].AutoFilter = true;
////        //        }
////        //        ws.Cells[ws.Dimension.Address].AutoFitColumns();
////        //}



////        //    [JetBrains.Annotations.NotNull]
////        //    private static ExcelPackage PrepareExcelPackage([JetBrains.Annotations.NotNull] string fileName)
////        //    {
////        //        if (File.Exists(fileName))
////        //        {
////        //            File.Delete(fileName);
////        //            Thread.Sleep(250);
////        //        }

////        //        var p = new ExcelPackage(new FileInfo(fileName));
////        //        return p;
////        //    }
////        }
////}
