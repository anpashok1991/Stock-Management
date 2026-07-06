using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

namespace StockManagement.Web.Services;

/// <summary>
/// Handles Blazor circuit errors and logs them for diagnostics
/// </summary>
public class CircuitErrorHandler : CircuitHandler
{
    private readonly ILogger<CircuitErrorHandler> _logger;

    public CircuitErrorHandler(ILogger<CircuitErrorHandler> logger)
    {
        _logger = logger;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit opened: {CircuitId}", circuit.Id);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit closed: {CircuitId}", circuit.Id);
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit connection established: {CircuitId}", circuit.Id);
        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Circuit connection down: {CircuitId}", circuit.Id);
        return base.OnConnectionDownAsync(circuit, cancellationToken);
    }
}
