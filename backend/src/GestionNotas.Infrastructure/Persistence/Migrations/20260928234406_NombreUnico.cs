using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionNotas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NombreUnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Profesor_Nombre",
                table: "Profesor");

            migrationBuilder.DropIndex(
                name: "IX_Estudiante_Nombre",
                table: "Estudiante");

            migrationBuilder.CreateIndex(
                name: "IX_Profesor_Nombre",
                table: "Profesor",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estudiante_Nombre",
                table: "Estudiante",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Profesor_Nombre",
                table: "Profesor");

            migrationBuilder.DropIndex(
                name: "IX_Estudiante_Nombre",
                table: "Estudiante");

            migrationBuilder.CreateIndex(
                name: "IX_Profesor_Nombre",
                table: "Profesor",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiante_Nombre",
                table: "Estudiante",
                column: "Nombre");
        }
    }
}
