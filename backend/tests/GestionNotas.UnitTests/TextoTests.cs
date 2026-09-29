using GestionNotas.Application.Common;

namespace GestionNotas.UnitTests;

public sealed class TextoTests
{
    [Theory]
    [InlineData("Samuel Torres", "Samuel Torres")]
    [InlineData("  Samuel Torres  ", "Samuel Torres")]
    [InlineData("Samuel    Torres", "Samuel Torres")]
    [InlineData("Samuel\tTorres", "Samuel Torres")]
    [InlineData("   ", "")]
    [InlineData(null, "")]
    public void Normalizar_QuitaEspaciosSobrantes(string? entrada, string esperado) =>
        Assert.Equal(esperado, Texto.Normalizar(entrada));
}
