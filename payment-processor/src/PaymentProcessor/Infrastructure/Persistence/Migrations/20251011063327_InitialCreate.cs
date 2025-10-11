using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentProcessor.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbox_events",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "text", nullable: false),
                    RawPayload = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_events", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "payment_event_records",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    EventAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_event_records", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "inbox_event_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InboxEventId = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OldStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NewStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AttemptNo = table.Column<int>(type: "integer", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    WorkerNode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_event_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inbox_event_logs_inbox_events_InboxEventId",
                        column: x => x.InboxEventId,
                        principalTable: "inbox_events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_event_logs_InboxEventId",
                table: "inbox_event_logs",
                column: "InboxEventId");

            migrationBuilder.CreateIndex(
                name: "IX_inbox_event_logs_InboxEventId_AttemptNo_NewStatus",
                table: "inbox_event_logs",
                columns: new[] { "InboxEventId", "AttemptNo", "NewStatus" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inbox_event_logs_InboxEventId_CreatedAt",
                table: "inbox_event_logs",
                columns: new[] { "InboxEventId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_event_logs_IsSuccess_CreatedAt",
                table: "inbox_event_logs",
                columns: new[] { "IsSuccess", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_event_logs_NewStatus_CreatedAt",
                table: "inbox_event_logs",
                columns: new[] { "NewStatus", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_events_Status",
                table: "inbox_events",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_inbox_events_UpdatedAt",
                table: "inbox_events",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_payment_event_records_EventAt",
                table: "payment_event_records",
                column: "EventAt");

            migrationBuilder.CreateIndex(
                name: "IX_payment_event_records_Status",
                table: "payment_event_records",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_event_logs");

            migrationBuilder.DropTable(
                name: "payment_event_records");

            migrationBuilder.DropTable(
                name: "inbox_events");
        }
    }
}
