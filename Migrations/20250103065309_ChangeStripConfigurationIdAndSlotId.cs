using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aurastrip_adapter.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStripConfigurationIdAndSlotId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SlotId",
                table: "Strips",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<Guid>(
                name: "ConfigurationId",
                table: "Strips",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigurationId",
                table: "Strips");

            migrationBuilder.AlterColumn<Guid>(
                name: "SlotId",
                table: "Strips",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
