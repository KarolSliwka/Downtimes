using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DowntimeTracker.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InfoRecords",
                columns: table => new
                {
                    InfoRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "VARCHAR(256)", nullable: false),
                    CorporateReason = table.Column<string>(type: "VARCHAR(256)", nullable: false),
                    M4Category = table.Column<string>(type: "VARCHAR(32)", nullable: false),
                    IsClaimable = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Example = table.Column<string>(type: "VARCHAR(1024)", nullable: false),
                    Explanation = table.Column<string>(type: "VARCHAR(1024)", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    Responsible = table.Column<string>(type: "VARCHAR(32)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfoRecords", x => x.InfoRecordId);
                });

            migrationBuilder.CreateTable(
                name: "MachineLineAreas",
                columns: table => new
                {
                    MachineLineAreaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<string>(type: "VARCHAR(256)", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR(128)", nullable: false),
                    OperatingTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineLineAreas", x => x.MachineLineAreaId);
                });

            migrationBuilder.CreateTable(
                name: "Personel",
                columns: table => new
                {
                    EmpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpNameSurname = table.Column<string>(type: "VARCHAR(256)", nullable: false),
                    EmpAd = table.Column<string>(type: "VARCHAR(64)", nullable: false),
                    EmpPosition = table.Column<string>(type: "VARCHAR(256)", nullable: false),
                    SupNameSurname = table.Column<string>(type: "VARCHAR(256)", nullable: false),
                    SupAd = table.Column<string>(type: "VARCHAR(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personel", x => x.EmpId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserAd = table.Column<string>(type: "VARCHAR(128)", nullable: false),
                    NameSurname = table.Column<string>(type: "VARCHAR(512)", nullable: false),
                    AccessLevel = table.Column<string>(type: "VARCHAR(128)", nullable: false),
                    HrmAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Downtimes",
                columns: table => new
                {
                    DowntimeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Week = table.Column<int>(type: "int", nullable: false),
                    Site = table.Column<string>(type: "VARCHAR(128)", nullable: false),
                    EventStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Department = table.Column<string>(type: "VARCHAR(256)", nullable: false),
                    Customer = table.Column<string>(type: "VARCHAR(128)", nullable: false),
                    Category = table.Column<string>(type: "VARCHAR(128)", nullable: false),
                    Reason = table.Column<string>(type: "VARCHAR(128)", nullable: false),
                    PeopleAffected = table.Column<int>(type: "int", nullable: false),
                    TotalHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsClaimable = table.Column<bool>(type: "bit", nullable: false),
                    Commentary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MachineLineAreaId = table.Column<int>(type: "int", nullable: false),
                    ApproverEmailADID = table.Column<string>(type: "VARCHAR(128)", nullable: true),
                    CreatedBy = table.Column<string>(type: "VARCHAR(256)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Claimed = table.Column<bool>(type: "bit", nullable: false),
                    ClaimedBy = table.Column<string>(type: "VARCHAR(256)", nullable: true),
                    ClaimedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Downtimes", x => x.DowntimeId);
                    table.ForeignKey(
                        name: "FK_Downtimes_MachineLineAreas_MachineLineAreaId",
                        column: x => x.MachineLineAreaId,
                        principalTable: "MachineLineAreas",
                        principalColumn: "MachineLineAreaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Downtimes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserLogs",
                columns: table => new
                {
                    UserLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentLogin = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogs", x => x.UserLogId);
                    table.ForeignKey(
                        name: "FK_UserLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Hrms",
                columns: table => new
                {
                    HrmId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Week = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<string>(type: "VARCHAR(256)", nullable: false),
                    Customer = table.Column<string>(type: "VARCHAR(128)", nullable: true),
                    Category = table.Column<string>(type: "VARCHAR(128)", nullable: false),
                    Reason = table.Column<string>(type: "VARCHAR(128)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeQty = table.Column<int>(type: "int", nullable: false),
                    TotalHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Commentary = table.Column<string>(type: "VARCHAR(500)", nullable: false),
                    IsUA = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "VARCHAR(256)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedById = table.Column<int>(type: "int", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Approver = table.Column<string>(type: "VARCHAR(256)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DowntimeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hrms", x => x.HrmId);
                    table.ForeignKey(
                        name: "FK_Hrms_Downtimes_DowntimeId",
                        column: x => x.DowntimeId,
                        principalTable: "Downtimes",
                        principalColumn: "DowntimeId");
                    table.ForeignKey(
                        name: "FK_Hrms_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Downtimes_MachineLineAreaId",
                table: "Downtimes",
                column: "MachineLineAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Downtimes_UserId",
                table: "Downtimes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Hrms_DowntimeId",
                table: "Hrms",
                column: "DowntimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Hrms_UserId",
                table: "Hrms",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogs_UserId",
                table: "UserLogs",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hrms");

            migrationBuilder.DropTable(
                name: "InfoRecords");

            migrationBuilder.DropTable(
                name: "Personel");

            migrationBuilder.DropTable(
                name: "UserLogs");

            migrationBuilder.DropTable(
                name: "Downtimes");

            migrationBuilder.DropTable(
                name: "MachineLineAreas");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
