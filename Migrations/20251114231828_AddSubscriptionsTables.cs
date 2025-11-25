using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Energix.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_personalizations",
                table: "personalizations");

            migrationBuilder.RenameTable(
                name: "personalizations",
                newName: "Personalizations");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Personalizations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Personalizations",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Personalizations",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "kpi_monthly",
                table: "Personalizations",
                newName: "KpiMonthly");

            migrationBuilder.RenameColumn(
                name: "kpi_current",
                table: "Personalizations",
                newName: "KpiCurrent");

            migrationBuilder.RenameColumn(
                name: "kpi_cost",
                table: "Personalizations",
                newName: "KpiCost");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Personalizations",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "chart_monthly",
                table: "Personalizations",
                newName: "ChartMonthly");

            migrationBuilder.RenameColumn(
                name: "chart_hourly",
                table: "Personalizations",
                newName: "ChartHourly");

            migrationBuilder.RenameColumn(
                name: "chart_device",
                table: "Personalizations",
                newName: "ChartDevice");

            migrationBuilder.RenameIndex(
                name: "IX_personalizations_user_id",
                table: "Personalizations",
                newName: "IX_Personalizations_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Personalizations",
                table: "Personalizations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MaskedCardNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CardBrand = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CardHolderName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlanType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingPeriod = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PriceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceCurrency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NextBillingDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    AutoRenew = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_UserId",
                table: "PaymentMethods",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_UserId_IsDefault",
                table: "PaymentMethods",
                columns: new[] { "UserId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Personalizations",
                table: "Personalizations");

            migrationBuilder.RenameTable(
                name: "Personalizations",
                newName: "personalizations");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "personalizations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "personalizations",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "personalizations",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "KpiMonthly",
                table: "personalizations",
                newName: "kpi_monthly");

            migrationBuilder.RenameColumn(
                name: "KpiCurrent",
                table: "personalizations",
                newName: "kpi_current");

            migrationBuilder.RenameColumn(
                name: "KpiCost",
                table: "personalizations",
                newName: "kpi_cost");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "personalizations",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ChartMonthly",
                table: "personalizations",
                newName: "chart_monthly");

            migrationBuilder.RenameColumn(
                name: "ChartHourly",
                table: "personalizations",
                newName: "chart_hourly");

            migrationBuilder.RenameColumn(
                name: "ChartDevice",
                table: "personalizations",
                newName: "chart_device");

            migrationBuilder.RenameIndex(
                name: "IX_Personalizations_UserId",
                table: "personalizations",
                newName: "IX_personalizations_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_personalizations",
                table: "personalizations",
                column: "id");
        }
    }
}
