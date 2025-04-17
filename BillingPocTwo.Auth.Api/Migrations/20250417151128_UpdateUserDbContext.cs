using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingPocTwo.Auth.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserUserRole_UserRoles_UserRoleId",
                table: "UserUserRole");

            migrationBuilder.DropForeignKey(
                name: "FK_UserUserRole_Users_UserId",
                table: "UserUserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserUserRole",
                table: "UserUserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserUserRole_UserRoleId",
                table: "UserUserRole");

            migrationBuilder.RenameColumn(
                name: "UserRoleId",
                table: "UserUserRole",
                newName: "SEQ_ROLE_MASTER");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "UserRoles",
                newName: "ROLE_ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserRoles",
                newName: "SEQ_ROLE_MASTER");

            migrationBuilder.AddColumn<long>(
                name: "CREATED_BY",
                table: "UserRoles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CREATED_ON",
                table: "UserRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EFFECTIVE_DATE",
                table: "UserRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EXPIRY_DATE",
                table: "UserRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IS_LOCKED",
                table: "UserRoles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LOCKED_REASON",
                table: "UserRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MODIFIED_BY",
                table: "UserRoles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MODIFIED_ON",
                table: "UserRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ROLE_DESCRIPTION",
                table: "UserRoles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ROWID",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserUserRole",
                table: "UserUserRole",
                columns: new[] { "SEQ_ROLE_MASTER", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserUserRole_UserId",
                table: "UserUserRole",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserUserRole_RoleMaster",
                table: "UserUserRole",
                column: "SEQ_ROLE_MASTER",
                principalTable: "UserRoles",
                principalColumn: "SEQ_ROLE_MASTER",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserUserRole_User",
                table: "UserUserRole",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserUserRole_RoleMaster",
                table: "UserUserRole");

            migrationBuilder.DropForeignKey(
                name: "FK_UserUserRole_User",
                table: "UserUserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserUserRole",
                table: "UserUserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserUserRole_UserId",
                table: "UserUserRole");

            migrationBuilder.DropColumn(
                name: "CREATED_BY",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "CREATED_ON",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "EFFECTIVE_DATE",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "EXPIRY_DATE",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "IS_LOCKED",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "LOCKED_REASON",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "MODIFIED_BY",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "MODIFIED_ON",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "ROLE_DESCRIPTION",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "ROWID",
                table: "UserRoles");

            migrationBuilder.RenameColumn(
                name: "SEQ_ROLE_MASTER",
                table: "UserUserRole",
                newName: "UserRoleId");

            migrationBuilder.RenameColumn(
                name: "ROLE_ID",
                table: "UserRoles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "SEQ_ROLE_MASTER",
                table: "UserRoles",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserUserRole",
                table: "UserUserRole",
                columns: new[] { "UserId", "UserRoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserUserRole_UserRoleId",
                table: "UserUserRole",
                column: "UserRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserUserRole_UserRoles_UserRoleId",
                table: "UserUserRole",
                column: "UserRoleId",
                principalTable: "UserRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserUserRole_Users_UserId",
                table: "UserUserRole",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
