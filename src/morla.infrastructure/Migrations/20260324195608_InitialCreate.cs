using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace morla.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Knowledges",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
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
                });

            // =====================
            // CREATE FTS5 VIRTUAL TABLE FOR FULL TEXT SEARCH
            // =====================
            migrationBuilder.Sql(@"
                CREATE VIRTUAL TABLE Knowledges_fts USING fts5(
                    Title,
                    Summary,
                    Content,
                    Topic,
                    content=Knowledges,
                    content_rowid=rowid
                );
            ");

            // Create trigger to keep FTS5 index in sync on INSERT
            migrationBuilder.Sql(@"
                CREATE TRIGGER Knowledges_ai AFTER INSERT ON Knowledges BEGIN
                  INSERT INTO Knowledges_fts(rowid, Title, Summary, Content, Topic) 
                  VALUES (new.rowid, new.Title, new.Summary, new.Content, new.Topic);
                END;
            ");

            // Create trigger to keep FTS5 index in sync on UPDATE
            migrationBuilder.Sql(@"
                CREATE TRIGGER Knowledges_au AFTER UPDATE ON Knowledges BEGIN
                  INSERT INTO Knowledges_fts(Knowledges_fts, rowid, Title, Summary, Content, Topic) 
                  VALUES('delete', old.rowid, old.Title, old.Summary, old.Content, old.Topic);
                  INSERT INTO Knowledges_fts(rowid, Title, Summary, Content, Topic) 
                  VALUES (new.rowid, new.Title, new.Summary, new.Content, new.Topic);
                END;
            ");

            // Create trigger to keep FTS5 index in sync on DELETE
            migrationBuilder.Sql(@"
                CREATE TRIGGER Knowledges_ad AFTER DELETE ON Knowledges BEGIN
                  INSERT INTO Knowledges_fts(Knowledges_fts, rowid, Title, Summary, Content, Topic) 
                  VALUES('delete', old.rowid, old.Title, old.Summary, old.Content, old.Topic);
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FTS5 triggers
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS Knowledges_ai;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS Knowledges_au;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS Knowledges_ad;");

            // Drop FTS5 virtual table
            migrationBuilder.Sql("DROP TABLE IF EXISTS Knowledges_fts;");

            // Drop main table
            migrationBuilder.DropTable(
                name: "Knowledges");
        }
    }
}
