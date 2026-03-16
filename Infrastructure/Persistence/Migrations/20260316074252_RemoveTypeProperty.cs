using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CmsFetchService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTypeProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "CmsRecordEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "CmsRecordEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
