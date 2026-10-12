using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarServer.Insfractructure.Migrations
{
    /// <inheritdoc />
    public partial class i_chance_datetimeoffset_to_datetimeonly_in_the_vehicle_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "LastMaintanceDate_Value",
                table: "Vehicles",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "InsuranceEndDate_Value",
                table: "Vehicles",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "InspectionDate_Value",
                table: "Vehicles",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CascoEndDate_Value",
                table: "Vehicles",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastMaintanceDate_Value",
                table: "Vehicles",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "InsuranceEndDate_Value",
                table: "Vehicles",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "InspectionDate_Value",
                table: "Vehicles",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CascoEndDate_Value",
                table: "Vehicles",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
