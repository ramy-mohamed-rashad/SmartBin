using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBin.Migrations
{
    /// <inheritdoc />
    public partial class AddCameraAndGps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrowdDensities_Bins_BinId",
                table: "CrowdDensities");

            migrationBuilder.DropForeignKey(
                name: "FK_SurroundingWastes_Bins_BinId",
                table: "SurroundingWastes");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "BinRequests");

            migrationBuilder.AlterColumn<int>(
                name: "BinId",
                table: "SurroundingWastes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CameraId",
                table: "SurroundingWastes",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BinId",
                table: "CrowdDensities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CameraId",
                table: "CrowdDensities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "BinRequests",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "BinRequests",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cameras",
                columns: table => new
                {
                    CameraId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentificationToken = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cameras", x => x.CameraId);
                    table.ForeignKey(
                        name: "FK_Cameras_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CameraRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CameraId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CameraRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CameraRequests_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "CameraId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurroundingWastes_CameraId",
                table: "SurroundingWastes",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_CrowdDensities_CameraId",
                table: "CrowdDensities",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_CameraRequests_CameraId",
                table: "CameraRequests",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_CameraRequests_UserId",
                table: "CameraRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_IdentificationToken",
                table: "Cameras",
                column: "IdentificationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_UserId",
                table: "Cameras",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CrowdDensities_Bins_BinId",
                table: "CrowdDensities",
                column: "BinId",
                principalTable: "Bins",
                principalColumn: "BinId");

            migrationBuilder.AddForeignKey(
                name: "FK_CrowdDensities_Cameras_CameraId",
                table: "CrowdDensities",
                column: "CameraId",
                principalTable: "Cameras",
                principalColumn: "CameraId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurroundingWastes_Bins_BinId",
                table: "SurroundingWastes",
                column: "BinId",
                principalTable: "Bins",
                principalColumn: "BinId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurroundingWastes_Cameras_CameraId",
                table: "SurroundingWastes",
                column: "CameraId",
                principalTable: "Cameras",
                principalColumn: "CameraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrowdDensities_Bins_BinId",
                table: "CrowdDensities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrowdDensities_Cameras_CameraId",
                table: "CrowdDensities");

            migrationBuilder.DropForeignKey(
                name: "FK_SurroundingWastes_Bins_BinId",
                table: "SurroundingWastes");

            migrationBuilder.DropForeignKey(
                name: "FK_SurroundingWastes_Cameras_CameraId",
                table: "SurroundingWastes");

            migrationBuilder.DropTable(
                name: "CameraRequests");

            migrationBuilder.DropTable(
                name: "Cameras");

            migrationBuilder.DropIndex(
                name: "IX_SurroundingWastes_CameraId",
                table: "SurroundingWastes");

            migrationBuilder.DropIndex(
                name: "IX_CrowdDensities_CameraId",
                table: "CrowdDensities");

            migrationBuilder.DropColumn(
                name: "CameraId",
                table: "SurroundingWastes");

            migrationBuilder.DropColumn(
                name: "CameraId",
                table: "CrowdDensities");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "BinRequests");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "BinRequests");

            migrationBuilder.AlterColumn<int>(
                name: "BinId",
                table: "SurroundingWastes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BinId",
                table: "CrowdDensities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "BinRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_CrowdDensities_Bins_BinId",
                table: "CrowdDensities",
                column: "BinId",
                principalTable: "Bins",
                principalColumn: "BinId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurroundingWastes_Bins_BinId",
                table: "SurroundingWastes",
                column: "BinId",
                principalTable: "Bins",
                principalColumn: "BinId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
