using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarServer.Insfractructure.Migrations
{
    /// <inheritdoc />
    public partial class i_added_Forgot_Password_fields_in_to_Customer_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ProtectionPackagePrice_Value",
                table: "Reservations",
                type: "money",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "money",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProtectionPackageId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ForgotPasswordCode_Value",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ForgotPasswordDate_Value",
                table: "Customers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsForgotPasswordCompleted_Value",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForgotPasswordCode_Value",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ForgotPasswordDate_Value",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsForgotPasswordCompleted_Value",
                table: "Customers");

            migrationBuilder.AlterColumn<decimal>(
                name: "ProtectionPackagePrice_Value",
                table: "Reservations",
                type: "money",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "money");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProtectionPackageId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
