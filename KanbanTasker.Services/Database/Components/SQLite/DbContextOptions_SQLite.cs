using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Sqlite;
using System.IO;
using Windows.Storage;

namespace KanbanTasker.Services.Database.Components.SQLite
{
    public class DbContextOptions_SQLite : IDbContextOptions
    {
        public DbContextOptions Options { get; set; }

        public DbContextOptions_SQLite(string connectionString)
        {
            // Microsoft.Data.Sqlite v3.0 requires full path. Crashes with SQLite Error 14 if not in this format
            // Check EndpointValidator.cs for comments and link to issue
            string dbFileName = connectionString.Split('=')[1];
            string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbFileName);

            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();
            builder.UseSqlite("Filename=" + dbPath);
            Options = builder.Options;
        }
    }
}
