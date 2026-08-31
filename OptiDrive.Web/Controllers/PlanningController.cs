using Microsoft.AspNetCore.Mvc;
using OptiDrive.Web.Infrastructure;
using OptiDrive.Web.Models;
using OptiDrive.Web.Services;

namespace OptiDrive.Web.Controllers;

public sealed class PlanningController(
    AppStateService appState,
    SmartSaveService smartSave,
    ExternalVehicleCatalogService vehicleCatalog,
    ExternalFuelStationService fuelStations,
    ExternalElectricStationService electricStations,
    IConfiguration configuration,
    ILogger<PlanningController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(Guid? routeId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        if (appState.IsAdmin(userId))
        {
            TempData["AdminWarning"] = "O planeamento é realizado no perfil de condutor. O backoffice mantém apenas a supervisão das rotas e viagens.";
            return RedirectToAction("Index", "Admin");
        }

        await vehicleCatalog.GetMakesAsync(cancellationToken);
        await fuelStations.GetAllStationsAsync(false, cancellationToken);

        var model = appState.BuildDashboard(userId.Value);
        var preferredVehicleId = model.Vehicles.FirstOrDefault(vehicle => vehicle.IsDefault)?.Id
                                 ?? model.Vehicles.FirstOrDefault()?.Id
                                 ?? Guid.Empty;
        var selectedRoute = routeId.HasValue
            ? model.Routes.FirstOrDefault(route => route.Id == routeId.Value)
            : model.Routes.FirstOrDefault();
        model.SelectedRouteId = selectedRoute?.Id;
        model.RouteInput.VehicleId = selectedRoute is not null && selectedRoute.VehicleId != Guid.Empty
            ? selectedRoute.VehicleId
            : preferredVehicleId;
        model.RouteInput.Origin = selectedRoute?.Origin ?? model.RouteInput.Origin;
        model.RouteInput.Destination = selectedRoute?.Destination ?? model.RouteInput.Destination;
        model.RouteInput.Waypoints = selectedRoute?.Waypoints ?? model.RouteInput.Waypoints;
        model.LatestRouteStops = selectedRoute is null ? [] : smartSave.ReadSuggestedStops(selectedRoute);

        ViewBag.Places = smartSave.PlaceNames;
        var browserMapEnabled = configuration.GetValue("GoogleMaps:BrowserEnabled", false);
        ViewBag.GoogleMapsApiKey = browserMapEnabled
            ? configuration["GoogleMaps:ApiKey"] ?? string.Empty
            : string.Empty;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> PlanRoute([Bind(Prefix = "RouteInput")] RoutePlanInputModel input, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        if (appState.IsAdmin(userId))
        {
            TempData["AdminWarning"] = "O backoffice acompanha rotas, mas a criação e aplicação de viagens pertence aos perfis de condutor.";
            return RedirectToAction("Index", "Admin");
        }

        input.Origin = input.Origin?.Trim() ?? string.Empty;
        input.Destination = input.Destination?.Trim() ?? string.Empty;
        input.Waypoints = input.Waypoints?.Trim();
        if (!ModelState.IsValid
            || string.IsNullOrWhiteSpace(input.Origin)
            || string.IsNullOrWhiteSpace(input.Destination))
        {
            TempData["PlanningError"] = "Preenche uma origem, um destino e uma velocidade média válidos.";
            return RedirectToAction(nameof(Index));
        }

        if (input.Origin.Equals(input.Destination, StringComparison.OrdinalIgnoreCase))
        {
            TempData["PlanningError"] = "A origem e o destino devem ser diferentes.";
            return RedirectToAction(nameof(Index));
        }

        var vehicle = appState.GetVehicle(userId.Value, input.VehicleId);
        if (vehicle is null)
        {
            TempData["PlanningError"] = "Seleciona um veículo válido para planear a rota.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var stations = (await fuelStations.GetAllStationsAsync(false, cancellationToken))
                .Concat(await electricStations.GetElectricStationsAsync(cancellationToken))
                .GroupBy(station => string.IsNullOrWhiteSpace(station.ExternalId) ? station.Name + "|" + station.Latitude + "|" + station.Longitude : station.ExternalId)
                .Select(group => group.First())
                .ToList();
            var route = await smartSave.BuildRouteAsync(userId.Value, vehicle, input, stations, cancellationToken);
            appState.SaveRoute(route);
            TempData["PlanningSuccess"] = "Rota calculada e guardada com sucesso.";
            return RedirectToAction(nameof(Index), new { routeId = route.Id });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falha ao calcular rota para o veiculo {VehicleId}.", input.VehicleId);
            TempData["PlanningError"] = "Não foi possível calcular a rota. Confirma os locais e tenta novamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ApplyRoute(Guid id)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        if (appState.IsAdmin(userId))
        {
            TempData["AdminWarning"] = "O backoffice acompanha rotas, mas a criação e aplicação de viagens pertence aos perfis de condutor.";
            return RedirectToAction("Index", "Admin");
        }

        var result = appState.ApplyRouteToVehicle(userId.Value, id);
        if (result.Success)
        {
            TempData["PlanningSuccess"] = result.Message;
        }
        else
        {
            TempData["PlanningError"] = result.Message;
        }

        return RedirectToAction(nameof(Index), new { routeId = id });
    }
}
