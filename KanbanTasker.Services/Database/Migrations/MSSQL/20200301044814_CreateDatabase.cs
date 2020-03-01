using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KanbanTasker.Services.Database.Migrations.MSSQL
{
    public partial class CreateDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblBoards",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBoards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblTags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TagName = table.Column<string>(nullable: true),
                    TagBackground = table.Column<string>(nullable: true),
                    TagForeground = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblTasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BoardId = table.Column<int>(nullable: false),
                    DateCreated = table.Column<string>(nullable: true),
                    DueDate = table.Column<string>(nullable: true),
                    StartDate = table.Column<string>(nullable: true),
                    FinishDate = table.Column<string>(nullable: true),
                    TimeDue = table.Column<string>(nullable: true),
                    ReminderTime = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Category = table.Column<string>(nullable: true),
                    ColumnIndex = table.Column<int>(nullable: false),
                    ColorKey = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblTasks_tblBoards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "tblBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskTags",
                columns: table => new
                {
                    TaskID = table.Column<int>(nullable: false),
                    TagID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTags", x => new { x.TaskID, x.TagID });
                    table.ForeignKey(
                        name: "FK_TaskTags_tblTags_TagID",
                        column: x => x.TagID,
                        principalTable: "tblTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskTags_tblTasks_TaskID",
                        column: x => x.TaskID,
                        principalTable: "tblTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskTags_TagID",
                table: "TaskTags",
                column: "TagID");

            migrationBuilder.CreateIndex(
                name: "IX_tblTasks_BoardId",
                table: "tblTasks",
                column: "BoardId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskTags");

            migrationBuilder.DropTable(
                name: "tblTags");

            migrationBuilder.DropTable(
                name: "tblTasks");

            migrationBuilder.DropTable(
                name: "tblBoards");
        }
    }
}
