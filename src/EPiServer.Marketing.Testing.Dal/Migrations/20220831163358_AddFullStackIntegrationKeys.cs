using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPiServer.Marketing.Testing.Dal.Migrations
{
    public partial class AddFullStackIntegrationKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblABTest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Owner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParticipationPercentage = table.Column<int>(type: "int", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpectedVisitorCount = table.Column<int>(type: "int", nullable: true),
                    ActualVisitorCount = table.Column<int>(type: "int", nullable: false),
                    ConfidenceLevel = table.Column<double>(type: "float", nullable: false),
                    ZScore = table.Column<double>(type: "float", nullable: false),
                    IsSignificant = table.Column<bool>(type: "bit", nullable: false),
                    ContentLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FS_FlagKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FS_ExperimentKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblABTest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblABKeyPerformanceIndicator",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    KeyPerformanceIndicatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblABKeyPerformanceIndicator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblABKeyPerformanceIndicator_tblABTest_TestId",
                        column: x => x.TestId,
                        principalTable: "tblABTest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblABVariant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemVersion = table.Column<int>(type: "int", nullable: false),
                    IsWinner = table.Column<bool>(type: "bit", nullable: false),
                    Conversions = table.Column<double>(type: "float", nullable: false),
                    Views = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblABVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblABVariant_tblABTest_TestId",
                        column: x => x.TestId,
                        principalTable: "tblABTest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblABKeyConversionResult",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KpiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Conversions = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    SelectedWeight = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Performance = table.Column<int>(type: "int", nullable: false),
                    VariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblABKeyConversionResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblABKeyConversionResult_tblABVariant_VariantId",
                        column: x => x.VariantId,
                        principalTable: "tblABVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblABKeyFinancialResult",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KpiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalMarketCulture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConvertedTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConvertedTotalCulture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblABKeyFinancialResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblABKeyFinancialResult_tblABVariant_VariantId",
                        column: x => x.VariantId,
                        principalTable: "tblABVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblABKeyValueResult",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KpiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    VariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblABKeyValueResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblABKeyValueResult_tblABVariant_VariantId",
                        column: x => x.VariantId,
                        principalTable: "tblABVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblABKeyConversionResult_VariantId",
                table: "tblABKeyConversionResult",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_tblABKeyFinancialResult_VariantId",
                table: "tblABKeyFinancialResult",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_tblABKeyPerformanceIndicator_TestId",
                table: "tblABKeyPerformanceIndicator",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_tblABKeyValueResult_VariantId",
                table: "tblABKeyValueResult",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_tblABVariant_TestId",
                table: "tblABVariant",
                column: "TestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblABKeyConversionResult");

            migrationBuilder.DropTable(
                name: "tblABKeyFinancialResult");

            migrationBuilder.DropTable(
                name: "tblABKeyPerformanceIndicator");

            migrationBuilder.DropTable(
                name: "tblABKeyValueResult");

            migrationBuilder.DropTable(
                name: "tblABVariant");

            migrationBuilder.DropTable(
                name: "tblABTest");
        }
    }
}
