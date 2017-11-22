using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Fan.Migrations
{
    public partial class FanSchemaV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blog_Category",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(maxLength: 256, nullable: false),
                    Title = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blog_Category", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "Blog_Tag",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Color = table.Column<string>(maxLength: 32, nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(maxLength: 256, nullable: false),
                    Title = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blog_Tag", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "Core_Meta",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(maxLength: 256, nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_Meta", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "Core_Role",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    IsSystemRole = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_User",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(nullable: false),
                    DisplayName = table.Column<string>(maxLength: 256, nullable: false),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    SecurityStamp = table.Column<string>(nullable: true),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_RoleClaim",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    RoleId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_RoleClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Core_RoleClaim_Core_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Core_Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Blog_Post",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Body = table.Column<string>(nullable: true),
                    BodyMark = table.Column<string>(nullable: true),
                    CategoryId = table.Column<int>(nullable: true),
                    CommentCount = table.Column<int>(nullable: false),
                    CommentStatus = table.Column<byte>(nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(nullable: false),
                    Excerpt = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: true),
                    RootId = table.Column<int>(nullable: true),
                    Slug = table.Column<string>(maxLength: 256, nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    Title = table.Column<string>(maxLength: 256, nullable: true),
                    Type = table.Column<byte>(nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    ViewCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blog_Post", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blog_Post_Blog_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Blog_Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Blog_Post_Core_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Core_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Core_Media",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AppId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(maxLength: 256, nullable: false),
                    Length = table.Column<long>(nullable: false),
                    MediaType = table.Column<byte>(nullable: false),
                    Title = table.Column<string>(maxLength: 256, nullable: true),
                    UploadedFrom = table.Column<byte>(nullable: false),
                    UploadedOn = table.Column<DateTimeOffset>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_Media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Core_Media_Core_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Core_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Core_UserClaim",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_UserClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Core_UserClaim_Core_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Core_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Core_UserLogin",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_UserLogin", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_Core_UserLogin_Core_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Core_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Core_UserRole",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    RoleId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_UserRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_Core_UserRole_Core_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Core_Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Core_UserRole_Core_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Core_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Core_UserToken",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_UserToken", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_Core_UserToken_Core_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Core_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Blog_PostTag",
                columns: table => new
                {
                    PostId = table.Column<int>(nullable: false),
                    TagId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blog_PostTag", x => new { x.PostId, x.TagId });
                    table.ForeignKey(
                        name: "FK_Blog_PostTag_Blog_Post_PostId",
                        column: x => x.PostId,
                        principalTable: "Blog_Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Blog_PostTag_Blog_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "Blog_Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blog_Category_Slug",
                table: "Blog_Category",
                column: "Slug",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Blog_Post_CategoryId",
                table: "Blog_Post",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_Post_ParentId",
                table: "Blog_Post",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_Post_Slug",
                table: "Blog_Post",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_Post_UserId",
                table: "Blog_Post",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_Post_Type_Status_CreatedOn_Id",
                table: "Blog_Post",
                columns: new[] { "Type", "Status", "CreatedOn", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Blog_PostTag_TagId",
                table: "Blog_PostTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_Tag_Slug",
                table: "Blog_Tag",
                column: "Slug",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_Media_UserId",
                table: "Core_Media",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Core_Media_MediaType_UploadedOn",
                table: "Core_Media",
                columns: new[] { "MediaType", "UploadedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Core_Meta_Key",
                table: "Core_Meta",
                column: "Key",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Core_Role",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Core_RoleClaim_RoleId",
                table: "Core_RoleClaim",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Core_User",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Core_User",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Core_UserClaim_UserId",
                table: "Core_UserClaim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Core_UserLogin_UserId",
                table: "Core_UserLogin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Core_UserRole_RoleId",
                table: "Core_UserRole",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blog_PostTag");

            migrationBuilder.DropTable(
                name: "Core_Media");

            migrationBuilder.DropTable(
                name: "Core_Meta");

            migrationBuilder.DropTable(
                name: "Core_RoleClaim");

            migrationBuilder.DropTable(
                name: "Core_UserClaim");

            migrationBuilder.DropTable(
                name: "Core_UserLogin");

            migrationBuilder.DropTable(
                name: "Core_UserRole");

            migrationBuilder.DropTable(
                name: "Core_UserToken");

            migrationBuilder.DropTable(
                name: "Blog_Post");

            migrationBuilder.DropTable(
                name: "Blog_Tag");

            migrationBuilder.DropTable(
                name: "Core_Role");

            migrationBuilder.DropTable(
                name: "Blog_Category");

            migrationBuilder.DropTable(
                name: "Core_User");
        }
    }
}
