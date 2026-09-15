using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPantry.Migrations
{
    /// <inheritdoc />
    public partial class AddPantryExpirationAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppPantryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPantryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppPantryItems_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppExpirationAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PantryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExpirationAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppExpirationAlerts_AppPantryItems_PantryItemId",
                        column: x => x.PantryItemId,
                        principalTable: "AppPantryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppExpirationAlerts_PantryItemId",
                table: "AppExpirationAlerts",
                column: "PantryItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppExpirationAlerts_UserId",
                table: "AppExpirationAlerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPantryItems_ProductId",
                table: "AppPantryItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPantryItems_UserId",
                table: "AppPantryItems",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppExpirationAlerts");

            migrationBuilder.DropTable(
                name: "AppPantryItems");
        }
    }
}
