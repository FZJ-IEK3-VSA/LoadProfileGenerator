using System;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database.Helpers;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Helpers
{

    public class DatabaseVersionCheckerTest: UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void RunatabaseVersionCheckerTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                DatabaseVersionChecker.CheckVersion(db.ConnectionString);
                string previousVersion = DatabaseVersionChecker.DstVersion;
                DatabaseVersionChecker.DstVersion = "asdf";
                Action a = () => DatabaseVersionChecker.CheckVersion(db.ConnectionString);
                a.Should().Throw<LPGException>();
                DatabaseVersionChecker.DstVersion = previousVersion;
            }
        }

        public DatabaseVersionCheckerTest([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
