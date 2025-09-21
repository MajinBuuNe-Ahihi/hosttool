using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HostTool.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchedulerDays",
                columns: table => new
                {
                    SchedulerDayId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SchedulerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Day = table.Column<int>(type: "INTEGER", nullable: false),
                    From = table.Column<string>(type: "TEXT", nullable: false),
                    To = table.Column<string>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerDays", x => x.SchedulerDayId);
                });

            migrationBuilder.CreateTable(
                name: "Schedulers",
                columns: table => new
                {
                    SchedulerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SchedulerName = table.Column<string>(type: "TEXT", nullable: false),
                    SchedulerDescription = table.Column<string>(type: "TEXT", nullable: false),
                    SchedulerPath = table.Column<string>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    RunAll = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedulers", x => x.SchedulerId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchedulerDays");

            migrationBuilder.DropTable(
                name: "Schedulers");
        }
    }
}
