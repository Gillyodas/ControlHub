using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Schema_ControlHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ControlHub");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "ControlHub");

            migrationBuilder.RenameTable(
                name: "Tokens",
                newName: "Tokens",
                newSchema: "ControlHub");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Roles",
                newSchema: "ControlHub");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                newName: "RolePermissions",
                newSchema: "ControlHub");

            migrationBuilder.RenameTable(
                name: "Permissions",
                newName: "Permissions",
                newSchema: "ControlHub");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                newName: "OutboxMessages",
                newSchema: "ControlHub");

            migrationBuilder.RenameTable(
                name: "Accounts",
                newName: "Accounts",
                newSchema: "ControlHub");

            migrationBuilder.RenameTable(
                name: "AccountIdentifiers",
                newName: "AccountIdentifiers",
                newSchema: "ControlHub");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Users",
                schema: "ControlHub",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Tokens",
                schema: "ControlHub",
                newName: "Tokens");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "ControlHub",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                schema: "ControlHub",
                newName: "RolePermissions");

            migrationBuilder.RenameTable(
                name: "Permissions",
                schema: "ControlHub",
                newName: "Permissions");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                schema: "ControlHub",
                newName: "OutboxMessages");

            migrationBuilder.RenameTable(
                name: "Accounts",
                schema: "ControlHub",
                newName: "Accounts");

            migrationBuilder.RenameTable(
                name: "AccountIdentifiers",
                schema: "ControlHub",
                newName: "AccountIdentifiers");
        }
    }
}
