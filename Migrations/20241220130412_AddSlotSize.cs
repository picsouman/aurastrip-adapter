using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aurastrip_adapter.Migrations
{
    /// <inheritdoc />
    public partial class AddSlotSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SizePercentage",
                table: "Slots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SizePercentage",
                table: "Slots");
        }
    }
}
