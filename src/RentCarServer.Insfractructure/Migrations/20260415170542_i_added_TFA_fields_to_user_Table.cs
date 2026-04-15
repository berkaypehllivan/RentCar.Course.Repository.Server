using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarServer.Insfractructure.Migrations
{
    /// <inheritdoc />
    public partial class i_added_TFA_fields_to_user_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token_value",
                table: "LoginToken",
                newName: "Token_Value");

            migrationBuilder.RenameColumn(
                name: "IsActive_value",
                table: "LoginToken",
                newName: "IsActive_Value");

            migrationBuilder.RenameColumn(
                name: "ExpiresDate_value",
                table: "LoginToken",
                newName: "ExpiresDate_Value");

            migrationBuilder.AddColumn<string>(
                name: "TFACode_Value",
                table: "Users",
                type: "nvarchar(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TFAConfirmCode_Value",
                table: "Users",
                type: "nvarchar(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TFAExpiresDate_Value",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TFAIsCompleted_Value",
                table: "Users",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TFAStatus_Value",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TFACode_Value",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TFAConfirmCode_Value",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TFAExpiresDate_Value",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TFAIsCompleted_Value",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TFAStatus_Value",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Token_Value",
                table: "LoginToken",
                newName: "Token_value");

            migrationBuilder.RenameColumn(
                name: "IsActive_Value",
                table: "LoginToken",
                newName: "IsActive_value");

            migrationBuilder.RenameColumn(
                name: "ExpiresDate_Value",
                table: "LoginToken",
                newName: "ExpiresDate_value");
        }
    }
}
