using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionNotas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NotaUnica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Si ya existen evaluaciones repetidas (creadas antes de esta regla), el índice único no se
            // podría crear. No se borra nada: las repetidas se renombran con un sufijo, p. ej.
            // «Parcial 1» → «Parcial 1 (2)», conservando la más antigua con su nombre original.
            migrationBuilder.Sql(@"
WITH Duplicadas AS
(
    SELECT Nombre,
           ROW_NUMBER() OVER (PARTITION BY IdEstudiante, IdProfesor, Nombre ORDER BY Id) AS Fila
    FROM dbo.Nota
)
UPDATE Duplicadas
SET Nombre = LEFT(Nombre, 94) + N' (' + CAST(Fila AS NVARCHAR(3)) + N')'
WHERE Fila > 1;");

            migrationBuilder.CreateIndex(
                name: "IX_Nota_IdEstudiante_IdProfesor_Nombre",
                table: "Nota",
                columns: new[] { "IdEstudiante", "IdProfesor", "Nombre" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Nota_IdEstudiante_IdProfesor_Nombre",
                table: "Nota");
        }
    }
}
