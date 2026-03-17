using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Removethetokenstringfromemailverificationtoken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailVerificationTokens_Token",
                table: "EmailVerificationTokens");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "EmailVerificationTokens");

            migrationBuilder.RenameColumn(
                name: "IsVerified",
                table: "Users",
                newName: "IsEmailVerified");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsEmailVerified",
                table: "Users",
                newName: "IsVerified");

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "EmailVerificationTokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationTokens_Token",
                table: "EmailVerificationTokens",
                column: "Token",
                unique: true);
        }
    }
}
