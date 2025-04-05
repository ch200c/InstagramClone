﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstagramClone.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBucketNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserBucketNames_UserId",
                table: "UserBucketNames",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserBucketNames_UserId",
                table: "UserBucketNames");
        }
    }
}
