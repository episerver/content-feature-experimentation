using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KPI.Migrations
{
    using System;
    
    [ExcludeFromCodeCoverage]
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dbo.tblKeyPerformaceIndicator",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ClassName = table.Column<String>(nullable: false),
                    Properties = table.Column<String>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                 {
                     table.PrimaryKey("PK_tblKeyPerformaceIndicator", t => t.Id);
                 });
                
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name:"dbo.tblKeyPerformaceIndicator");
        }
    }
}
