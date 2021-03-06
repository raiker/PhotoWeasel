﻿using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Data.SQLite;

namespace PhotoWeaselDatabase.Management
{
    public static class DatabaseManager
    {
        public static Database CreatePWDB(string location)
        {
            //Delete file first if it already exists
            try
            {
                File.Delete(location);
            }
            catch (DirectoryNotFoundException) { ;}

            SQLiteConnection.CreateFile(location);

            //produce the connection string and open DB
            string conStr = GetConnectionString(location);
            Database db = OpenPWDB(conStr);
            //Create tables
            db.CreateTables();
            return db;
        }

        public static Database OpenPWDB(string connectionString)
        {
            return new Database(connectionString);
        }

        public static string GetConnectionString(string dbfile, string conStringName = "Default")
        {
            var resources = new ResourceManager("PhotoWeaselDatabase.ConnectionStrings", Assembly.GetExecutingAssembly());

            string storedConString = resources.GetString(conStringName);
            if (storedConString == null)
                throw new DatabaseConfigurationException("There is no connection string named " + conStringName + " in the resource file");

            //Rebuild connection string based on this string, with dbfile inserted
            var newString = new SQLiteConnectionStringBuilder(storedConString);
            newString.DataSource = dbfile;
            return newString.ConnectionString;
        }
    }
}
