using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FhirProject.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInputSourceTypeAndExtractionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExtractionConfidence",
                table: "ConversionRequests",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractionWarnings",
                table: "ConversionRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InputSourceType",
                table: "ConversionRequests",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtractionConfidence",
                table: "ConversionRequests");

            migrationBuilder.DropColumn(
                name: "ExtractionWarnings",
                table: "ConversionRequests");

            migrationBuilder.DropColumn(
                name: "InputSourceType",
                table: "ConversionRequests");
        }
    }
}
