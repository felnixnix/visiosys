using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visiosys.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaAndamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "andamentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    precatorio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    registrado_por_login = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ocorrido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_andamentos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_andamentos_precatorio_id",
                table: "andamentos",
                column: "precatorio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "andamentos");
        }
    }
}
