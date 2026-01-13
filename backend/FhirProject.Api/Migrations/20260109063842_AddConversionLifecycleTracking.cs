using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FhirProject.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddConversionLifecycleTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "ConversionRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureStage",
                table: "ConversionRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FhirCreatedAt",
                table: "ConversionRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FhirValidatedAt",
                table: "ConversionRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NormalizedAt",
                table: "ConversionRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StoredAt",
                table: "ConversionRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TerminologyMappedAt",
                table: "ConversionRequests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "ConversionRequests");

            migrationBuilder.DropColumn(
                name: "FailureStage",
                table: "ConversionRequests");

            migrationBuilder.DropColumn(
                name: "FhirCreatedAt",
                table: "ConversionRequests");

            migrationBuilder.DropColumn(
                name: "FhirValidatedAt",
                table: "ConversionRequests");

            migrationBuilder.DropColumn(
                name: "NormalizedAt",
                table: "ConversionRequests");

            migrationBuilder.DropColumn(
                name: "StoredAt",
                table: "ConversionRequests");

            migrationBuilder.DropColumn(
                name: "TerminologyMappedAt",
                table: "ConversionRequests");
        }
    }
}
