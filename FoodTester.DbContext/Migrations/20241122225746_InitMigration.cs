using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodTester.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class InitMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalysisTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodBatches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: false),
                    SerialNumber = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Samples",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlogId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Samples", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnalysisTypeId = table.Column<long>(type: "bigint", nullable: false),
                    BatchId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisRequests_AnalysisTypes_AnalysisTypeId",
                        column: x => x.AnalysisTypeId,
                        principalTable: "AnalysisTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalysisRequests_FoodBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "FoodBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResultData = table.Column<string>(type: "VARCHAR(MAX)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisResults_AnalysisRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AnalysisRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_AnalysisTypeId",
                table: "AnalysisRequests",
                column: "AnalysisTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_BatchId",
                table: "AnalysisRequests",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisResults_RequestId",
                table: "AnalysisResults",
                column: "RequestId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisResults");

            migrationBuilder.DropTable(
                name: "Samples");

            migrationBuilder.DropTable(
                name: "AnalysisRequests");

            migrationBuilder.DropTable(
                name: "AnalysisTypes");

            migrationBuilder.DropTable(
                name: "FoodBatches");
        }
    }
}
