using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentifierConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdentifierConfigs",
                schema: "ControlHub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentifierConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentifierValidationRules",
                schema: "ControlHub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParametersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IdentifierConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentifierValidationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentifierValidationRules_IdentifierConfigs_IdentifierConfigId",
                        column: x => x.IdentifierConfigId,
                        principalSchema: "ControlHub",
                        principalTable: "IdentifierConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdentifierValidationRules_IdentifierConfigId",
                schema: "ControlHub",
                table: "IdentifierValidationRules",
                column: "IdentifierConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentifierValidationRules_Order",
                schema: "ControlHub",
                table: "IdentifierValidationRules",
                column: "Order");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentifierValidationRules",
                schema: "ControlHub");

            migrationBuilder.DropTable(
                name: "IdentifierConfigs",
                schema: "ControlHub");
        }
    }
}
