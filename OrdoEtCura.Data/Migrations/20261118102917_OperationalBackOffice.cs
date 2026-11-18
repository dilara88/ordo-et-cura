using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdoEtCura.Web.Migrations
{
    /// <inheritdoc />
    public partial class OperationalBackOffice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
            migrationBuilder.Sql("UPDATE AspNetUsers SET IsActive = 1");

            migrationBuilder.CreateTable(
                name: "ProjectEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Actor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEvents", x => x.Id);
                });
            migrationBuilder.Sql("INSERT INTO ProjectEvents (ProjectId, OccurredAt, Actor, Description) SELECT Id, COALESCE(ApprovedAt, SYSDATETIMEOFFSET()), 'System', CASE WHEN ApprovedAt IS NULL THEN 'Existing project imported with pending approval.' ELSE 'Existing approval imported. Original approver details were not recorded.' END FROM Projects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectEvents");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");
        }
    }
}
