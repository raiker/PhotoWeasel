using System.Data.SQLite;
using PhotoWeaselDatabase.Management;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PhotoWeasalDatabaseTests
{
    
    
    /// <summary>
    ///This is a test class for DatabaseTest and is intended
    ///to contain all DatabaseTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DatabaseManagerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for GetConnectionString
        ///</summary>
        [TestMethod()]
        public void GetConnectionStringDefaultTest()
        {
            const string fileName = "|DataDirectory|foo.s3b";
            var expectedDefaultConStr =
                new SQLiteConnectionStringBuilder("Data Source=|DataDirectory|foo.s3b;Version=3;");


            string returnedConStr = DatabaseManager.GetConnectionString(fileName);
            var builder = new SQLiteConnectionStringBuilder(returnedConStr);

            Assert.AreEqual(builder.ConnectionString, expectedDefaultConStr.ConnectionString);
        }

        [TestMethod()]
        public void GetConnectionStringROTest()
        {
            const string fileName = "|DataDirectory|foo.s3b";
            var expectedROConStr =
                new SQLiteConnectionStringBuilder("Data Source=|DataDirectory|foo.s3b;Version=3;Read Only=True;");

            string returnedConStr = DatabaseManager.GetConnectionString(fileName, "ReadOnly");
            var builder = new SQLiteConnectionStringBuilder(returnedConStr);

            Assert.AreEqual(builder.ConnectionString, expectedROConStr.ConnectionString);
        }

        [TestMethod()]
        [ExpectedException(typeof(DatabaseConfigurationException))]
        public void GetConnectionStringUnknownTest()
        {
            DatabaseManager.GetConnectionString("foo.s3b", "Unknown");
        }

    }
}
