using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestionNotas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Estudiante",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estudiante", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profesor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profesor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nota",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdProfesor = table.Column<int>(type: "int", nullable: false),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nota", x => x.Id);
                    table.CheckConstraint("CK_Nota_Valor", "[Valor] >= 0 AND [Valor] <= 5");
                    table.ForeignKey(
                        name: "FK_Nota_Estudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nota_Profesor",
                        column: x => x.IdProfesor,
                        principalTable: "Profesor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Estudiante",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Sofía Ramírez" },
                    { 2, "Santiago Gómez" },
                    { 3, "Valentina Muñoz" },
                    { 4, "Mateo Rojas" },
                    { 5, "Isabella Díaz" },
                    { 6, "Samuel Torres" },
                    { 7, "Mariana Vargas" },
                    { 8, "Sebastián Peña" },
                    { 9, "Camila Ortiz" },
                    { 10, "Nicolás Jiménez" },
                    { 11, "Daniela Ríos" },
                    { 12, "Tomás Álvarez" }
                });

            migrationBuilder.InsertData(
                table: "Profesor",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Carlos Rodríguez" },
                    { 2, "María Fernanda López" },
                    { 3, "Jorge Iván Martínez" },
                    { 4, "Ana Lucía Herrera" },
                    { 5, "Andrés Felipe Castaño" }
                });

            migrationBuilder.InsertData(
                table: "Nota",
                columns: new[] { "Id", "IdEstudiante", "IdProfesor", "Nombre", "Valor" },
                values: new object[,]
                {
                    { 1, 1, 2, "Parcial 2", 3.5m },
                    { 2, 1, 3, "Taller de álgebra", 2.9m },
                    { 3, 1, 4, "Quiz de programación", 3.8m },
                    { 4, 2, 3, "Taller de álgebra", 4.7m },
                    { 5, 2, 4, "Quiz de programación", 1.8m },
                    { 6, 2, 5, "Examen final", 2.4m },
                    { 7, 3, 4, "Quiz de programación", 4.2m },
                    { 8, 3, 5, "Examen final", 2.4m },
                    { 9, 3, 1, "Exposición", 3.5m },
                    { 10, 4, 5, "Examen final", 4.5m },
                    { 11, 4, 1, "Exposición", 1.8m },
                    { 12, 4, 2, "Proyecto integrador", 4.2m },
                    { 13, 5, 1, "Exposición", 3.0m },
                    { 14, 5, 2, "Proyecto integrador", 1.8m },
                    { 15, 5, 3, "Parcial 1", 2.4m },
                    { 16, 6, 2, "Proyecto integrador", 3.8m },
                    { 17, 6, 3, "Parcial 1", 3.8m },
                    { 18, 6, 4, "Parcial 2", 2.4m },
                    { 19, 7, 3, "Parcial 1", 3.0m },
                    { 20, 7, 4, "Parcial 2", 2.4m },
                    { 21, 8, 4, "Parcial 2", 4.2m },
                    { 22, 8, 5, "Taller de álgebra", 3.8m },
                    { 23, 9, 5, "Taller de álgebra", 1.8m },
                    { 24, 9, 1, "Quiz de programación", 4.5m },
                    { 25, 10, 1, "Quiz de programación", 2.4m },
                    { 26, 10, 2, "Examen final", 3.0m },
                    { 27, 11, 2, "Examen final", 4.7m },
                    { 28, 11, 3, "Exposición", 4.7m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Estudiante_Nombre",
                table: "Estudiante",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Nota_IdEstudiante",
                table: "Nota",
                column: "IdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_Nota_IdProfesor",
                table: "Nota",
                column: "IdProfesor");

            migrationBuilder.CreateIndex(
                name: "IX_Profesor_Nombre",
                table: "Profesor",
                column: "Nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Nota");

            migrationBuilder.DropTable(
                name: "Estudiante");

            migrationBuilder.DropTable(
                name: "Profesor");
        }
    }
}
