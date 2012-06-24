using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Reflection;

namespace PhotoWeaselDatabase.Management
{
    public enum DatabaseSQLStatements
    {
        CreateDatabase = 0
    }

    public class Database
    {
        private SQLiteConnection connection;
        private Dictionary<DatabaseSQLStatements, SQLiteCommand> preparedStatements;

        public Database(string connectionString)
        {
            //Open a database using the supplied connection string
            connection = new SQLiteConnection(connectionString);

            //Prepare statements using sql files in resources
            preparedStatements = new Dictionary<DatabaseSQLStatements, SQLiteCommand>();

            string createDatabaseString = GetSQLResource("PhotoWeaselDatabase.SQL.CreateDatabase.sql");
            var createDatabaseCommand = new SQLiteCommand(connection);
            createDatabaseCommand.CommandText = createDatabaseString;
            //no parameters for creating the database
            preparedStatements.Add(DatabaseSQLStatements.CreateDatabase, createDatabaseCommand);
        }

        private static string GetSQLResource(string SQLName)
        {
            var resourceStream = new MemoryStream();
            var assemblyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SQLName);
            if (assemblyStream == null)
                throw new DatabaseConfigurationException("Could not find the resource " + SQLName);
            assemblyStream.CopyTo(resourceStream);
            byte[] resourceBytes = resourceStream.ToArray();
            var encoder = new UTF8Encoding(true, true); //without byte order mark

            //Strip the BOM out of the resource if it's present
            byte[] bom = encoder.GetPreamble();
            bool haveBom = true;
            for (int i = 0; i < bom.Length; i++)
            {
                if (resourceBytes[i] != bom[i])
                    haveBom = false;
            }
            int startOffset = haveBom ? bom.Length : 0;
            

            return encoder.GetString(resourceBytes, startOffset, resourceBytes.Length - startOffset);
        }

        public void CreateTables()
        {
            //no need for transaction, the database is empty here
            preparedStatements[DatabaseSQLStatements.CreateDatabase].ExecuteNonQuery();
        }
    }
}
