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
    public class DatabaseTest
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
        ///A test for GetSQLResource
        ///</summary>
        [TestMethod()]
        [DeploymentItem("PhotoWeaselDatabase.dll")]
        public void GetSQLResourceTest()
        {
            string expected = "Test Resource";
            
            string actual = Database_Accessor.GetSQLResource("PhotoWeaselDatabase.SQL.TestResource.txt");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [ExpectedException(typeof(DatabaseConfigurationException))]
        public void GetSQLResourceFailTest()
        {
            Database_Accessor.GetSQLResource("IDoNotExist");
        }
    }
}
