using Automation;
using Common;
using Common.Extensions;
using Common.Tests;
using Database.Database;
using Database.Tables.Transportation;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Database.Tests
{
    public class SharedConnectionTests(ITestOutputHelper testOutputHelper) : UnitTestBaseClass(testOutputHelper)
    {
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void SharedConnectionCommitsAndRollsBack()
        {
            using DatabaseSetup db = new(Utili.GetCurrentMethodAndClass());
            using (var scope = Connection.BeginSharedConnection(db.ConnectionString))
            {
                var site = new Site("committed site", null, db.ConnectionString, "desc", true, System.Guid.NewGuid().ToStrGuid());
                site.SaveToDB();
                // nested transactions on the shared connection must work, as they are used by some SaveToDB implementations
                using (var con = new Connection(db.ConnectionString))
                {
                    con.Open();
                    using var tr = con.BeginTransaction();
                    new Site("nested site", null, db.ConnectionString, "desc", true, System.Guid.NewGuid().ToStrGuid()).SaveToDB(con);
                    tr.Commit();
                }
                scope.Commit();
            }
            using (Connection.BeginSharedConnection(db.ConnectionString))
            {
                var site = new Site("rolled back site", null, db.ConnectionString, "desc", true, System.Guid.NewGuid().ToStrGuid());
                site.SaveToDB();
                // no commit, so this site must be rolled back
            }
            using (Connection.BeginSharedConnection(db.ConnectionString))
            {
                // a committed nested transaction alone must not persist anything if the scope itself is not committed
                using var con = new Connection(db.ConnectionString);
                con.Open();
                using var tr = con.BeginTransaction();
                new Site("nested commit only", null, db.ConnectionString, "desc", true, System.Guid.NewGuid().ToStrGuid()).SaveToDB(con);
                tr.Commit();
            }
            // an exception inside a nested transaction must roll back everything and must not cause another exception when disposing the scope
            var failingScope = () =>
            {
                using var scope = Connection.BeginSharedConnection(db.ConnectionString);
                using var con = new Connection(db.ConnectionString);
                con.Open();
                using var tr = con.BeginTransaction();
                new Site("nested exception", null, db.ConnectionString, "desc", true, System.Guid.NewGuid().ToStrGuid()).SaveToDB(con);
                throw new System.InvalidOperationException("test exception");
            };
            failingScope.Should().Throw<System.InvalidOperationException>().WithMessage("test exception");
            // a new scope must be possible afterwards
            using (var scope = Connection.BeginSharedConnection(db.ConnectionString))
            {
                scope.Commit();
            }

            // check that the sites are in the database as expected
            var sim = new Simulator(db.ConnectionString);
            sim.Sites.FindFirstByName("committed site").Should().NotBeNull();
            sim.Sites.FindFirstByName("nested site").Should().NotBeNull();
            sim.Sites.FindFirstByName("rolled back site").Should().BeNull();
            sim.Sites.FindFirstByName("nested commit only").Should().BeNull();
            sim.Sites.FindFirstByName("nested exception").Should().BeNull();
            db.Cleanup();
        }
    }
}
