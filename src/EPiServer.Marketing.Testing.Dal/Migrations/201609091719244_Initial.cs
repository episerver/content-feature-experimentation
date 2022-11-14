using System.Diagnostics.CodeAnalysis;

namespace Testing.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;
    using System;
    
    [ExcludeFromCodeCoverage]
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dbo.tblABTest",
                columns: table => new
                    {
                        Id = table.Column<Guid>(nullable: false),
                        Title = table.Column<String>(nullable: false),
                        Description = table.Column<String>(),
                        Owner = table.Column<String>(nullable: false),
                        OriginalItemId = table.Column<Guid>(nullable: false),
                        State = table.Column<int>(nullable: false),
                        StartDate = table.Column<DateTime>(nullable: false),
                        EndDate = table.Column<DateTime>(nullable: false),
                        ParticipationPercentage = table.Column<int>(nullable: false),
                        LastModifiedBy = table.Column<String>(maxLength: 100),
                        ExpectedVisitorCount = table.Column<int>(),
                        ActualVisitorCount = table.Column<int>(nullable: false),
                        ConfidenceLevel = table.Column<Double>(nullable: false),
                        ZScore = table.Column<Double>(nullable: false),
                        IsSignificant = table.Column<Boolean>(nullable: false),
                        CreatedDate = table.Column<DateTime>(nullable: false),
                        ModifiedDate = table.Column<DateTime>(nullable: false),
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblABTest", t => t.Id);
                });

            migrationBuilder.CreateTable(
                name: "dbo.tblABKeyPerformanceIndicator",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TestId = table.Column<Guid>(nullable: false),
                    KeyPerformanceIndicatorId = table.Column<Guid>(),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblABKeyPerformanceIndicator", t => t.Id);
                    table.ForeignKey(
                        name: "FK_tblABKeyPerformanceIndicator_tblABTest",
                        column: t => t.TestId,
                        principalTable: "dbo.tblABTest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblABKeyPerformanceIndicator_TestId",
                table: "dbo.tblABKeyPerformanceIndicator",
                column: "TestId");

            migrationBuilder.CreateTable(
                name: "dbo.tblABVariant",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TestId = table.Column<Guid>(nullable: false),
                    ItemId = table.Column<Guid>(nullable: false),
                    ItemVersion = table.Column<int>(nullable: false),
                    IsWinner = table.Column<Boolean>(nullable: false),
                    Conversions = table.Column<int>(nullable: false),
                    Views = table.Column<int>(nullable: false),
                    IsPublished = table.Column<Boolean>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblABVariant", t => t.Id);
                    table.ForeignKey(
                        name: "FK_tblABVariant_tblABTest",
                        column: t => t.TestId,
                        principalTable: "dbo.tblABTest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblABVariant_TestId",
                table: "dbo.tblABVariant",
                column: "TestId");

        }
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "dbo.tblABVariant");
            migrationBuilder.DropTable(name: "dbo.tblABKeyPerformanceIndicator");
            migrationBuilder.DropTable(name: "dbo.tblABTest");
        }
    }
}
