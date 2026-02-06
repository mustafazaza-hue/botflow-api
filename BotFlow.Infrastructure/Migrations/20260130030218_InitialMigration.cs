using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BotFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIDataSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FileUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FileType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    QueryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DocumentCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ApiEndpoint = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    DatabaseType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ProgressPercentage = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIDataSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIDataSources_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPIMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MetricType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Period = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<double>(type: "REAL", precision: 18, scale: 4, nullable: false),
                    TargetValue = table.Column<double>(type: "REAL", precision: 18, scale: 4, nullable: false),
                    ChangePercentage = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: false),
                    Platform = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPIMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveSubscriptions = table.Column<int>(type: "INTEGER", nullable: false),
                    TrialUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    SuspendedUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalBots = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalDocuments = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveDataSources = table.Column<int>(type: "INTEGER", nullable: false),
                    ProcessingDataSources = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedDataSources = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UXPilotApiCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WhatsAppApiCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvgResponseTime = table.Column<double>(type: "REAL", precision: 10, scale: 2, nullable: false),
                    ServerUptime = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: false),
                    DatabaseLoad = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: false),
                    BotSuccessRate = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: false),
                    ErrorRate = table.Column<double>(type: "REAL", precision: 5, scale: 4, nullable: false),
                    TotalComments = table.Column<int>(type: "INTEGER", nullable: false),
                    MessagesSent = table.Column<int>(type: "INTEGER", nullable: false),
                    UXPilotApiCalls = table.Column<int>(type: "INTEGER", nullable: false),
                    WhatsAppApiCalls = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemStatistics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIDataSources_CreatedAt",
                table: "AIDataSources",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIDataSources_Status",
                table: "AIDataSources",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AIDataSources_Type",
                table: "AIDataSources",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AIDataSources_Type_Status",
                table: "AIDataSources",
                columns: new[] { "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AIDataSources_UserId",
                table: "AIDataSources",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDataSources_UserId_Status",
                table: "AIDataSources",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KPIMetrics_Date",
                table: "KPIMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_KPIMetrics_MetricType",
                table: "KPIMetrics",
                column: "MetricType");

            migrationBuilder.CreateIndex(
                name: "IX_KPIMetrics_MetricType_Date",
                table: "KPIMetrics",
                columns: new[] { "MetricType", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_KPIMetrics_MetricType_Period_Date",
                table: "KPIMetrics",
                columns: new[] { "MetricType", "Period", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_KPIMetrics_MetricType_Platform_Date",
                table: "KPIMetrics",
                columns: new[] { "MetricType", "Platform", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPIMetrics_Period",
                table: "KPIMetrics",
                column: "Period");

            migrationBuilder.CreateIndex(
                name: "IX_KPIMetrics_Platform",
                table: "KPIMetrics",
                column: "Platform");

            migrationBuilder.CreateIndex(
                name: "IX_SystemStatistics_CreatedAt",
                table: "SystemStatistics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SystemStatistics_Date",
                table: "SystemStatistics",
                column: "Date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIDataSources");

            migrationBuilder.DropTable(
                name: "KPIMetrics");

            migrationBuilder.DropTable(
                name: "SystemStatistics");
        }
    }
}
