﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace P225Allup.Migrations
{
    public partial class UpdatedUsersTabelAddedColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeActive",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsDeActive",
                table: "AspNetUsers");
        }
    }
}
