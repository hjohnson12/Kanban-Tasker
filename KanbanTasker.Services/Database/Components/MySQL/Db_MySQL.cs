using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Services.Database.Components.MySQL
{
    public class Db_MySQL : Db, IMigrationContext
    {
        public Db_MySQL(DbContextOptions options) : base(options)
        {

        }
    }
}
