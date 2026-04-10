// Migrations/XXXXXXXXXX_AddSaleRecords.cs
using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SolarRent.Migrations
{
    public partial class AddSaleRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaleRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    ManagedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleRecords_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleRecords_Users_ManagedByUserId",
                        column: x => x.ManagedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SaleItemRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleRecordId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleItemRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleItemRecords_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleItemRecords_SaleRecords_SaleRecordId",
                        column: x => x.SaleRecordId,
                        principalTable: "SaleRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleItemRecords_EquipmentId",
                table: "SaleItemRecords",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItemRecords_SaleRecordId",
                table: "SaleItemRecords",
                column: "SaleRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleRecords_ClientId",
                table: "SaleRecords",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleRecords_ManagedByUserId",
                table: "SaleRecords",
                column: "ManagedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleRecords_SaleDate",
                table: "SaleRecords",
                column: "SaleDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SaleItemRecords");
            migrationBuilder.DropTable(name: "SaleRecords");
        }
    }
}