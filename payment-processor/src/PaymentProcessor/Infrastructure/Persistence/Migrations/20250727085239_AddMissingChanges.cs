using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentProcessor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingStartedAt",
                table: "inbox_events");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "inbox_events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProcessingStartedAt",
                table: "inbox_events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "inbox_events",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
