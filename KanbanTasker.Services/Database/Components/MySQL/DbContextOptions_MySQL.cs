using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Pomelo.EntityFrameworkCore.MySql;

namespace KanbanTasker.Services.Database.Components.MySQL
{
    public class DbContextOptions_MySQL : IDbContextOptions
    {
        public DbContextOptions Options { get; set; }

        public DbContextOptions_MySQL(string connectionString)
        {
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();
            builder.UseMySql(connectionString);
            Options = builder.Options;
        }
    }
}
