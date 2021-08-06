using Microsoft.EntityFrameworkCore.Migrations;

namespace KanbanTasker.Services.Database.Migrations.SQLite
{
    public partial class CreateColumnsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "tblColumns",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        BoardId = table.Column<int>(nullable: false),
            //        ColumnName = table.Column<string>(nullable: true),
            //        Position = table.Column<int>(nullable: false),
            //        MaxTaskLimit = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_tblColumns", x => x.Id);
            //    });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "tblColumns");
        }
    }
}
