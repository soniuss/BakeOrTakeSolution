using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.ApiRest.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    id_cliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_registro = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ubicacion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.id_cliente);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    id_empresa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fecha_registro = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre_negocio = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ubicacion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.id_empresa);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Recetas",
                columns: table => new
                {
                    id_receta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ingredientes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pasos = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    imagenUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_cliente_creador = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recetas", x => x.id_receta);
                    table.ForeignKey(
                        name: "FK_Recetas_Clientes_id_cliente_creador",
                        column: x => x.id_cliente_creador,
                        principalTable: "Clientes",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    id_cliente = table.Column<int>(type: "int", nullable: false),
                    id_receta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => new { x.id_cliente, x.id_receta });
                    table.ForeignKey(
                        name: "FK_Favoritos_Clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "Clientes",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Recetas_id_receta",
                        column: x => x.id_receta,
                        principalTable: "Recetas",
                        principalColumn: "id_receta",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PedidosOfertas",
                columns: table => new
                {
                    id_pedido_oferta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    precio = table.Column<double>(type: "double", nullable: false),
                    disponibilidad = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    descripcionOferta = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_empresa = table.Column<int>(type: "int", nullable: false),
                    id_receta = table.Column<int>(type: "int", nullable: false),
                    id_cliente = table.Column<int>(type: "int", nullable: true),
                    fechaPedido = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    estado = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    puntuacion = table.Column<int>(type: "int", nullable: true),
                    comentario = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fechaValoracion = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosOfertas", x => x.id_pedido_oferta);
                    table.ForeignKey(
                        name: "FK_PedidosOfertas_Clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "Clientes",
                        principalColumn: "id_cliente");
                    table.ForeignKey(
                        name: "FK_PedidosOfertas_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidosOfertas_Recetas_id_receta",
                        column: x => x.id_receta,
                        principalTable: "Recetas",
                        principalColumn: "id_receta",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_id_receta",
                table: "Favoritos",
                column: "id_receta");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosOfertas_id_cliente",
                table: "PedidosOfertas",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosOfertas_id_empresa",
                table: "PedidosOfertas",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosOfertas_id_receta",
                table: "PedidosOfertas",
                column: "id_receta");

            migrationBuilder.CreateIndex(
                name: "IX_Recetas_id_cliente_creador",
                table: "Recetas",
                column: "id_cliente_creador");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favoritos");

            migrationBuilder.DropTable(
                name: "PedidosOfertas");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropTable(
                name: "Recetas");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
