using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FhirProject.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToConversionRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ConversionRequests",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ConversionRequests");
        }
    }
}
