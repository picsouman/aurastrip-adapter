using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aurastrip_adapter.Migrations
{
    /// <inheritdoc />
    public partial class AddToStripClearedTakeOff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ClearedTakeOff",
                table: "Strips",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClearedTakeOff",
                table: "Strips");
        }
    }
}
