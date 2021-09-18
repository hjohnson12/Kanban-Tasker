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
        public DbSet<BoardDTO> Boards { get; set; }
        public DbSet<TaskDTO> Tasks { get; set; }

        public Db(Func<IDbContextOptions> dbContextOptionsFactory) : base(dbContextOptionsFactory().Options) { }

        public Db(DbContextOptions options): base(options) { }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.Entity<BoardDTO>().ToTable("tblBoards");
            mb.Entity<TaskDTO>().ToTable("tblTasks");
            mb.Entity<ColumnDTO>().ToTable("tblColumns");
        }
    }
}
