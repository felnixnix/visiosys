using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visiosys.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "precatorios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tribunal_origem = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor_face = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_atualizado = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    esfera = table.Column<int>(type: "integer", nullable: false),
                    natureza = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precatorios", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_precatorios_numero",
                table: "precatorios",
                column: "numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "precatorios");
        }
    }
}
