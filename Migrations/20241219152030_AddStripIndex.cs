using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aurastrip_adapter.Migrations
{
    /// <inheritdoc />
    public partial class AddStripIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "Strips",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "Strips");
        }
    }
}
