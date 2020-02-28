using Microsoft.EntityFrameworkCore.Migrations;

namespace KanbanTasker.Services.Database.Migrations.SQLite
{
    public partial class CreateDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            // NOTE: In the case where db is already created locally, just comment this out
            #region DEBUG
            //migrationBuilder.CreateTable(
            //    name: "tblBoards",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        Name = table.Column<string>(nullable: true),
            //        Notes = table.Column<string>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_tblBoards", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "tblTasks",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        BoardId = table.Column<int>(nullable: false),
            //        DateCreated = table.Column<string>(nullable: true),
            //        DueDate = table.Column<string>(nullable: true),
            //        StartDate = table.Column<string>(nullable: true),
            //        FinishDate = table.Column<string>(nullable: true),
            //        TimeDue = table.Column<string>(nullable: true),
            //        ReminderTime = table.Column<string>(nullable: true),
            //        Title = table.Column<string>(nullable: true),
            //        Description = table.Column<string>(nullable: true),
            //        Category = table.Column<string>(nullable: true),
            //        ColumnIndex = table.Column<int>(nullable: false),
            //        ColorKey = table.Column<string>(nullable: true),
            //        Tags = table.Column<string>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_tblTasks", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_tblTasks_tblBoards_BoardId",
            //            column: x => x.BoardId,
            //            principalTable: "tblBoards",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_tblTasks_BoardId",
            //    table: "tblTasks",
            //    column: "BoardId");

           
            #endregion DEBUG

            // Add new columns to db
            migrationBuilder.AddColumn<string>(
                name: "DueDate",
                table: "tblTasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinishDate",
                table: "tblTasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeDue",
                table: "tblTasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReminderTime",
                table: "tblTasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartDate",
                table: "tblTasks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "tblTasks");

            migrationBuilder.DropColumn(
                name: "TimeDue",
                table: "tblTasks");

            migrationBuilder.DropColumn(
                name: "FinishDate",
                table: "tblTasks");

            migrationBuilder.DropColumn(
                name: "ReminderTime",
                table: "tblTasks");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "tblTasks");
        }
    }
}
