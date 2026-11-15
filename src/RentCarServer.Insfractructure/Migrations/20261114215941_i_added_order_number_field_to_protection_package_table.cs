using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarServer.Insfractructure.Migrations
{
    /// <inheritdoc />
    public partial class i_added_order_number_field_to_protection_package_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderNumber_Value",
                table: "ProtectionPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNumber_Value",
                table: "ProtectionPackages");
        }
    }
}
