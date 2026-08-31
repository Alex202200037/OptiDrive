using Microsoft.AspNetCore.Mvc;
using OptiDrive.Web.Infrastructure;
using OptiDrive.Web.Models;
using OptiDrive.Web.Services;

namespace OptiDrive.Web.Controllers;

public sealed class DashboardController(
    AppStateService appState,
    SmartSaveService smartSave,
    ExternalFuelStationService fuelStations,
    ExternalElectricStationService electricStations) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        return appState.IsAdmin(userId)
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Profile");
    }

    [HttpPost]
    public IActionResult AddVehicle(VehicleInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        appState.AddVehicle(userId.Value, input);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult DeleteVehicle(Guid id)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        appState.RemoveVehicle(userId.Value, id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult SetDefaultVehicle(Guid id)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        appState.SetDefaultVehicle(userId.Value, id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> PlanRoute(RoutePlanInputModel input, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        var vehicle = appState.GetVehicle(userId.Value, input.VehicleId);
        if (vehicle is null)
        {
            TempData["Error"] = "Seleciona um veiculo valido.";
            return RedirectToAction(nameof(Index));
        }

        var stations = (await fuelStations.GetAllStationsAsync(false, cancellationToken))
            .Concat(await electricStations.GetElectricStationsAsync(cancellationToken))
            .GroupBy(station => string.IsNullOrWhiteSpace(station.ExternalId) ? station.Name + "|" + station.Latitude + "|" + station.Longitude : station.ExternalId)
            .Select(group => group.First())
            .ToList();
        var route = await smartSave.BuildRouteAsync(userId.Value, vehicle, input, stations, cancellationToken);
        appState.SaveRoute(route);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult AddContact(ContactInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        appState.AddContact(userId.Value, input);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ShareVehicle(ShareVehicleInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        appState.ShareVehicle(userId.Value, input);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult CreateGroup(GroupInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        appState.AddGroup(userId.Value, input);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ReportPrice(PriceReportInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        appState.AddReport(userId.Value, input);
        return RedirectToAction(nameof(Index));
    }
}
