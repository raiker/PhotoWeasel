using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using PhotoWeasel.domain;
using System.Data.SQLite;

namespace PWDBTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var cfg = new Configuration();
            cfg.Configure();
            cfg.AddAssembly(typeof(Photo).Assembly);

            new SchemaExport(cfg).Execute(false, true, false);
            Console.ReadLine();
        }
    }
}
