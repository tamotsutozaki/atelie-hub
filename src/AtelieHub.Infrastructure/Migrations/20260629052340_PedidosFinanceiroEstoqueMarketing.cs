using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtelieHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PedidosFinanceiroEstoqueMarketing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lancamentos_financeiros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lancamentos_financeiros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lancamentos_financeiros_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pedidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Prioridade = table.Column<int>(type: "integer", nullable: false),
                    Urgente = table.Column<bool>(type: "boolean", nullable: false),
                    DataPedido = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PrazoEntrega = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StatusPagamento = table.Column<int>(type: "integer", nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Transportadora = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CodigoRastreio = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    DataPostagem = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pedidos_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pedidos_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "produtos_estoque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Quantidade = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    EstoqueMinimo = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Custo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produtos_estoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_produtos_estoque_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tarefas_marketing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Rede = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    DataPrevista = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Concluido = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarefas_marketing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tarefas_marketing_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lancamentos_financeiros_EmpresaId_Data",
                table: "lancamentos_financeiros",
                columns: new[] { "EmpresaId", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_ClienteId",
                table: "pedidos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_EmpresaId_Numero",
                table: "pedidos",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_EmpresaId_Status",
                table: "pedidos",
                columns: new[] { "EmpresaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_produtos_estoque_EmpresaId_Nome",
                table: "produtos_estoque",
                columns: new[] { "EmpresaId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_tarefas_marketing_EmpresaId_DataPrevista",
                table: "tarefas_marketing",
                columns: new[] { "EmpresaId", "DataPrevista" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lancamentos_financeiros");

            migrationBuilder.DropTable(
                name: "pedidos");

            migrationBuilder.DropTable(
                name: "produtos_estoque");

            migrationBuilder.DropTable(
                name: "tarefas_marketing");
        }
    }
}
