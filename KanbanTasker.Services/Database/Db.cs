using System;
using System.Collections.Generic;
using System.Text;
using KanbanTasker.Model;
using KanbanTasker.Model.Dto;
using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KanbanTasker.Services.Database
{
    public class Db : DbContext
    {
        public DbSet<BoardDto> Boards { get; set; }
        public DbSet<TaskDto> Tasks { get; set; }

        public Db(Func<IDbContextOptions> dbContextOptionsFactory) : base(dbContextOptionsFactory().Options) { }

        public Db(DbContextOptions options): base(options) { }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.Entity<BoardDto>().ToTable("tblBoards");
            mb.Entity<TaskDto>().ToTable("tblTasks");
            mb.Entity<ColumnDto>().ToTable("tblColumns");
        }
    }
}
