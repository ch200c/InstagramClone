using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstagramClone.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserIdInUserBucketName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdentityUserId",
                table: "UserBucketNames",
                newName: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserBucketNames",
                newName: "IdentityUserId");
        }
    }
}
