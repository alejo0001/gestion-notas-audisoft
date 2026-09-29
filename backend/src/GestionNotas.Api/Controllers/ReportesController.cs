using GestionNotas.Application.Reportes;
using Microsoft.AspNetCore.Mvc;

namespace GestionNotas.Api.Controllers;

[Route("api/reportes")]
public sealed class ReportesController(IReporteService service) : ApiControllerBase
{
    /// <summary>Indicadores generales para el panel de inicio.</summary>
    [HttpGet("resumen")]
    [ProducesResponseType<ResumenDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenDto>> GetResumen(CancellationToken cancellationToken) =>
        Ok(await service.GetResumenAsync(cancellationToken));
}
