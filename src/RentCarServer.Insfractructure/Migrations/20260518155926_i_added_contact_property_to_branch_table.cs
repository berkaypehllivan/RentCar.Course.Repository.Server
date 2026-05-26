using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarServer.Insfractructure.Migrations
{
    /// <inheritdoc />
    public partial class i_added_contact_property_to_branch_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address_PhoneNumber2",
                table: "Branches",
                newName: "Contact_PhoneNumber2");

            migrationBuilder.RenameColumn(
                name: "Address_PhoneNumber1",
                table: "Branches",
                newName: "Contact_PhoneNumber1");

            migrationBuilder.RenameColumn(
                name: "Address_Email",
                table: "Branches",
                newName: "Contact_Email");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Contact_PhoneNumber2",
                table: "Branches",
                newName: "Address_PhoneNumber2");

            migrationBuilder.RenameColumn(
                name: "Contact_PhoneNumber1",
                table: "Branches",
                newName: "Address_PhoneNumber1");

            migrationBuilder.RenameColumn(
                name: "Contact_Email",
                table: "Branches",
                newName: "Address_Email");
        }
    }
}
