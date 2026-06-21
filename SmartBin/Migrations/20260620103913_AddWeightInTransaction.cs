using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBin.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightInTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "weight",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "weight",
                table: "Transactions");
        }
    }
}
