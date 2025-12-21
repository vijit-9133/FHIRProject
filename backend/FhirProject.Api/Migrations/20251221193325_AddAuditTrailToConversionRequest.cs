using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FhirProject.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTrailToConversionRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "ConversionRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MappingVersion",
                table: "ConversionRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "v1");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ConversionRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "ConversionRequests");

            migrationBuilder.DropColumn(
                name: "MappingVersion",
                table: "ConversionRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ConversionRequests");
        }
    }
}
