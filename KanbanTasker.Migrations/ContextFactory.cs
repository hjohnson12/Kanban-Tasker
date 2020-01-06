using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using KanbanTasker.Model;
using LeaderAnalytics.AdaptiveClient;
using KanbanTasker.Services.Database.Components.MSSQL;
using KanbanTasker.Services.Database.Components.MySQL;

namespace KanbanTasker.Migrations
{
    // https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dbcontext-creation

    public class MSSQL_ContextFactory : IDesignTimeDbContextFactory<Db_MSSQL>
    {
        public Db_MSSQL CreateDbContext(string[] args)
        {
            string fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EndPoints.json");
            string connectionString = EndPointUtilities.LoadEndPoints(fileName, false).First(x => x.API_Name == API_Name.Kanban && x.ProviderName == DatabaseProvider.MSSQL).ConnectionString;
            DbContextOptionsBuilder dbOptions = new DbContextOptionsBuilder();
            dbOptions.UseSqlServer(connectionString);
            Db_MSSQL db = new Db_MSSQL(dbOptions.Options);
            return db;
        }
    }

    public class MySQL_ContextFactory : IDesignTimeDbContextFactory<Db_MySQL>
    {
        public Db_MySQL CreateDbContext(string[] args)
        {
            string fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EndPoints.json");
            string connectionString = EndPointUtilities.LoadEndPoints(fileName, false).First(x => x.API_Name == API_Name.Kanban && x.ProviderName == DatabaseProvider.MySQL).ConnectionString;
            DbContextOptionsBuilder dbOptions = new DbContextOptionsBuilder();
            dbOptions.UseMySql(connectionString);
            Db_MySQL db = new Db_MySQL(dbOptions.Options);
            return db;
        }
    }
}
