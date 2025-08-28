using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth0_Blazor.Migrations
{
    /// <inheritdoc />
    public partial class AddWaterFrequencyToPlant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WaterFrequency",
                table: "Plants",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WaterFrequency",
                table: "Plants");
        }
    }
}
