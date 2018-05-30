using Microsoft.EntityFrameworkCore.Migrations;

namespace Fan.Migrations
{
    public partial class FanV1_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Blog_Post",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256);

            migrationBuilder.RenameColumn(
                name: "AppId",
                table: "Core_Media",
                newName: "AppType");

            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "Core_Media",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "Core_Media",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "Core_Media",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Core_Media",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Optimized",
                table: "Core_Media",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
              name: "Slug",
              table: "Blog_Post",
              maxLength: 256,
              nullable: false,
              oldClrType: typeof(string),
              oldMaxLength: 256,
              oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Core_Media");

            migrationBuilder.DropColumn(
                name: "Caption",
                table: "Core_Media");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "Core_Media");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Core_Media");

            migrationBuilder.DropColumn(
                name: "Optimized",
                table: "Core_Media");

            migrationBuilder.RenameColumn(
                name: "AppType",
                table: "Core_Media",
                newName: "AppId");
        }
    }
}