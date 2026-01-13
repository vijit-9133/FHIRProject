using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FhirProject.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalResourceMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalResourceMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResourceType = table.Column<int>(type: "int", nullable: false),
                    InternalResourceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalResourceMappings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalResourceMappings_SourceSystem_ExternalId",
                table: "ExternalResourceMappings",
                columns: new[] { "SourceSystem", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalResourceMappings");
        }
    }
}
