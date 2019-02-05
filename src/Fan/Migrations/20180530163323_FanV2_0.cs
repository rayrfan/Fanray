using Microsoft.EntityFrameworkCore.Migrations;

namespace Fan.Migrations
{
    public partial class FanV2_0 : Migration
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
                name: "ContentType",
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

            migrationBuilder.AddColumn<string>(
                name: "Alt",
                table: "Core_Media",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResizeCount",
                table: "Core_Media",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Core_User",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256);

            migrationBuilder.DropPrimaryKey(
              name: "PK_Core_Meta",
              table: "Core_Meta");

            migrationBuilder.DropIndex(
                name: "IX_Core_Meta_Key",
                table: "Core_Meta");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Core_Meta",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Core_Meta",
                table: "Core_Meta",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Core_Meta_Type",
                table: "Core_Meta",
                column: "Type");

            migrationBuilder.Sql("UPDATE [Core_Meta] SET [Key] = 'blogsettings.allowcomments' WHERE [Key] = 'blogsettings.allowcommentsonblogpost';");
            migrationBuilder.Sql("UPDATE [Core_Meta] SET [Key] = 'blogsettings.feedshowexcerpt' WHERE [Key] = 'blogsettings.rssshowexcerpt';");
            migrationBuilder.Sql("UPDATE [Core_Meta] SET [Key] = 'blogsettings.postperpage' WHERE [Key] = 'blogsettings.pagesize';");
            migrationBuilder.Sql("UPDATE [Core_Meta] SET [Value] = 1 WHERE [Key] = 'blogsettings.showexcerpt';");
            migrationBuilder.Sql("UPDATE [Core_Meta] SET [Key] = 'blogsettings.postlistdisplay' WHERE [Key] = 'blogsettings.showexcerpt';");
            migrationBuilder.Sql("DELETE FROM [Core_Meta] WHERE [Key] = 'blogsettings.excerptwordlimit';");
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
                name: "ContentType",
                table: "Core_Media");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Core_Media");

            migrationBuilder.DropColumn(
                name: "Alt",
                table: "Core_Media");

            migrationBuilder.DropColumn(
                name: "ResizeCount",
                table: "Core_Media");

            migrationBuilder.RenameColumn(
                name: "AppType",
                table: "Core_Media",
                newName: "AppId");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Core_User",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.DropPrimaryKey(
              name: "PK_Core_Meta",
              table: "Core_Meta");

            migrationBuilder.DropIndex(
                name: "IX_Core_Meta_Type",
                table: "Core_Meta");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Core_Meta");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Core_Meta",
                table: "Core_Meta",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Core_Meta_Key",
                table: "Core_Meta",
                column: "Key",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.Sql("UPDATE [Core_Meta] SET [Key] = 'blogsettings.allowcommentsonblogpost' WHERE [Key] = 'blogsettings.allowcomments';");
            migrationBuilder.Sql("UPDATE [Core_Meta] SET [Key] = 'blogsettings.rssshowexcerpt' WHERE [Key] = 'blogsettings.feedshowexcerpt';");
            migrationBuilder.Sql("UPDATE [Core_Meta] SET [Key] = 'blogsettings.pagesize' WHERE [Key] = 'blogsettings.postperpage';");
            migrationBuilder.Sql("UPDATE [Core_Meta] SET [Value] = 'False' WHERE [Key] = 'blogsettings.postlistdisplay';");
            migrationBuilder.Sql("UPDATE [Core_Meta] SET [Key] = 'blogsettings.showexcerpt' WHERE [Key] = 'blogsettings.postlistdisplay';");
            migrationBuilder.Sql("INSERT INTO [Core_Meta] VALUES('blogsettings.excerptwordlimit', 55);");
        }
    }
}