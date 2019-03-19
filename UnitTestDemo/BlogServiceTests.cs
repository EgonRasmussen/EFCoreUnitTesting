// Projekt af typen MSTest
// Tilføj NuGet pakken: Microsoft.EntityFrameworkCore.InMemory til projektet med Contexten
// Husk forhindre contexten i at benytte SQL server konfigurationen.

// Microsoft Docs: https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting?view=mstest-net-1.2.0
// Fin blog om MSTest: https://www.meziantou.net/2018/01/22/mstest-v2-setup-a-test-project-and-run-tests

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DataLayer;
using ServiceLayer;
using QueryingData.Models;

namespace UnitTestDemo
{
    [TestClass]
    public class BlogServiceTests
    {
        [TestMethod]
        public void Add_writes_to_database()
        {
            // ARRANGE: Create option object with SQL server
            var options = new DbContextOptionsBuilder<BloggingContext>()
            //  .UseInMemoryDatabase(databaseName: "Add_writes_to_database")
                .UseSqlServer("Server = (localdb)\\mssqllocaldb; Database = BloggingDb; Trusted_Connection = True;")
                .Options;

            // ARRANGE: Empty new DB
            using (var context = new BloggingContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            // ACT: Run the test against one instance of the context
            using (var context = new BloggingContext(options))
            {
                var service = new BlogService(context);
                service.Add("http://sample.com");
            }

            // ASSERT: Use a separate instance of the context to verify correct data was saved to database
            using (var context = new BloggingContext(options))
            {
                Assert.AreEqual(1, context.Blogs.Count());
                Assert.AreEqual("http://sample.com", context.Blogs.Single().Url);
            }
        }

        [TestMethod]
        public void Find_searches_url()
        {
            var options = new DbContextOptionsBuilder<BloggingContext>()
                //.UseInMemoryDatabase(databaseName: "Find_searches_url")
                .UseSqlServer("Server = (localdb)\\mssqllocaldb; Database = BloggingDb; Trusted_Connection = True;")
                .Options;

            // Insert seed data into the database using one instance of the context
            using (var context = new BloggingContext(options))
            {
                context.Blogs.Add(new Blog { Url = "http://sample.com/cats" });
                context.Blogs.Add(new Blog { Url = "http://sample.com/catfish" });
                context.Blogs.Add(new Blog { Url = "http://sample.com/dogs" });
                context.SaveChanges();
            }

            // Use a clean instance of the context to run the test
            using (var context = new BloggingContext(options))
            {
                var service = new BlogService(context);
                var result = service.Find("cat");
                Assert.AreEqual(2, result.Count());
            }
        }
    }
}
