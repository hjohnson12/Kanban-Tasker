using System;
using System.Collections.Generic;
using System.Text;
using KanbanTasker.Model;
using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KanbanTasker.Services.Database
{
    public class Db : DbContext
    {
        public DbSet<BoardDTO> Boards { get; set; }
        public DbSet<TaskDTO> Tasks { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TaskTag> TaskTags { get; set; }

        public Db(Func<IDbContextOptions> dbContextOptionsFactory) : base(dbContextOptionsFactory().Options) { }

        public Db(DbContextOptions options): base(options) { }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.Entity<BoardDTO>().ToTable("tblBoards");
            mb.Entity<TaskDTO>().ToTable("tblTasks");
            mb.Entity<Tag>().ToTable("tblTags");

            mb.Entity<TaskTag>().HasOne(x => x.Tag)
                .WithMany(x => x.TaskTags)
                .HasForeignKey(x => x.TagID)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<TaskTag>().HasOne(x => x.Task)
                 .WithMany(x => x.TaskTags)
                 .HasForeignKey(x => x.TaskID)
                 .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
