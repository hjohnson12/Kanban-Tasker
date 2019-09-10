using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using KanbanTasker.Services.Database;

namespace KanbanTasker.Services.Database.Components.MSSQL
{
    public class DatabaseInitializer : IDatabaseInitializer
    {

        private Db db;


        public DatabaseInitializer(Db db)
        {
            this.db = db;
        }

        public async Task Seed(string migrationName)
        {

        }
    }
}
