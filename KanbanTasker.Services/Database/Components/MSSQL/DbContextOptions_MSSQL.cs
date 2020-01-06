using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Services.Database.Components.MSSQL
{
    public class DbContextOptions_MSSQL : IDbContextOptions
    {
        public DbContextOptions Options { get; set; }

        public DbContextOptions_MSSQL(string connectionString)
        {
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();
            builder.UseSqlServer(connectionString);
            Options = builder.Options;
        }
    }
}
