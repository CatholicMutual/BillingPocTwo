using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingPocTwo.Auth.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddChangePasswordColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ChangePasswordOnFirstLogin",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangePasswordOnFirstLogin",
                table: "Users");
        }
    }
}
