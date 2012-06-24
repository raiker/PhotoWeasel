using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotoWeaselDatabase.Management
{
    public class DatabaseConfigurationException : Exception
    {
        public DatabaseConfigurationException() : base()
        {
        }

        public DatabaseConfigurationException(string msg) : base(msg)
        {
        }

        public DatabaseConfigurationException(string msg, Exception innerEx) : base(msg, innerEx)
        {
        }
    }
}
