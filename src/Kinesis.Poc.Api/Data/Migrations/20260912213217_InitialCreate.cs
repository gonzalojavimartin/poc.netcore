using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kinesis.Poc.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                    table.CheckConstraint("CK_Patients_DocumentPair", "([DocumentType] IS NULL AND [DocumentNumber] IS NULL) OR ([DocumentType] IS NOT NULL AND [DocumentNumber] IS NOT NULL AND LEN(LTRIM(RTRIM([DocumentType]))) > 0 AND LEN(LTRIM(RTRIM([DocumentNumber]))) > 0)");
                    table.CheckConstraint("CK_Patients_FirstName", "LEN(LTRIM(RTRIM([FirstName]))) > 0");
                    table.CheckConstraint("CK_Patients_LastName", "LEN(LTRIM(RTRIM([LastName]))) > 0");
                    table.CheckConstraint("CK_Patients_Status", "[Status] IN (N'Active', N'Inactive')");
                });

            migrationBuilder.CreateIndex(
                name: "UX_Patients_Document",
                table: "Patients",
                columns: new[] { "DocumentType", "DocumentNumber" },
                unique: true,
                filter: "[DocumentType] IS NOT NULL AND [DocumentNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
