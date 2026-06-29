using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visiosys.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_original = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    chave_armazenamento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    url_download = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    tamanho_bytes = table.Column<long>(type: "bigint", nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    enviado_por_login = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    precatorio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documentos", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documentos");
        }
    }
}
