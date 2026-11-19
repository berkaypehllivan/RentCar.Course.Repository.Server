using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarServer.Insfractructure.Migrations
{
    /// <inheritdoc />
    public partial class i_changed_address_field_to_customer_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address_Value",
                table: "Customers",
                newName: "Address_FullAddress");

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "Customers",
                type: "nvarchar(MAX)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_District",
                table: "Customers",
                type: "nvarchar(MAX)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Address_District",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "Address_FullAddress",
                table: "Customers",
                newName: "Address_Value");
        }
    }
}
