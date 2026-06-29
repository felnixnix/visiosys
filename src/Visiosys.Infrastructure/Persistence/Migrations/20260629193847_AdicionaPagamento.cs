using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visiosys.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pagamentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    precatorio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_base = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    perc_desagio = table.Column<decimal>(type: "numeric(7,4)", nullable: false),
                    registrado_por_login = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    pago_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagamentos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pagamentos_precatorio_id",
                table: "pagamentos",
                column: "precatorio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pagamentos");
        }
    }
}
