using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace morla.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToKnowledge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Knowledges",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Knowledges",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "idx_knowledges_isdeleted",
                table: "Knowledges",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_knowledges_isdeleted",
                table: "Knowledges");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Knowledges");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Knowledges");
        }
    }
}
