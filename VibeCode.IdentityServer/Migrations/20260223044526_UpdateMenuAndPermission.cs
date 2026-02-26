using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeCode.IdentityServer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMenuAndPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "auth",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "auth",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "auth",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "auth",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "auth",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "auth",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "auth",
                table: "Menus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "auth",
                table: "Permissions",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "auth",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "auth",
                table: "Permissions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "auth",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "auth",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "auth",
                table: "Permissions",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "auth",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "auth",
                table: "Menus",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "auth",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "auth",
                table: "Menus",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "auth",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "auth",
                table: "Menus",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "auth",
                table: "Menus",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "auth",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
