using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace morla.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Knowledges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RowId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Topic = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Project = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Knowledges", x => x.Id);
                    table.UniqueConstraint("AK_Knowledges_RowId", x => x.RowId);
                });

            // Tabla para embeddings - múltiples por Knowledge
            // id: PRIMARY KEY autoincrement
            // rowid: referencia a Knowledge.Id (permite duplicados)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS knowledges_embedding (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    rowid INTEGER NOT NULL,
                    embedding BLOB NOT NULL,
                    FOREIGN KEY (rowid) REFERENCES Knowledges(Id) ON DELETE CASCADE
                );
            ");

            // Índice para búsquedas por Knowledge.Id
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_knowledges_embedding_rowid 
                ON knowledges_embedding(rowid);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Knowledges");

            // Eliminar tabla de embeddings
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS knowledges_embedding;
            ");
        }
    }
}
