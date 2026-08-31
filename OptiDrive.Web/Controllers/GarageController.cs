using Microsoft.AspNetCore.Mvc;
using OptiDrive.Web.Infrastructure;
using OptiDrive.Web.Models;
using OptiDrive.Web.Services;

namespace OptiDrive.Web.Controllers;

public sealed class GarageController(
    AppStateService appState,
    ExternalVehicleCatalogService vehicleCatalog,
    ExternalFuelStationService fuelStations) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(Guid? vehicleId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        var makes = await vehicleCatalog.GetMakesAsync(cancellationToken);
        await fuelStations.GetAllStationsAsync(false, cancellationToken);
        var model = appState.BuildDashboard(userId.Value);
        model.VehicleMakes = makes;
        model.SelectedVehicleId = vehicleId
            ?? model.Vehicles.FirstOrDefault(vehicle => vehicle.IsDefault)?.Id
            ?? model.Vehicles.FirstOrDefault()?.Id;
        return View(model);
    }

    [HttpPost]
    public IActionResult AddVehicle([Bind(Prefix = "VehicleInput")] VehicleInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        if (!ModelState.IsValid)
        {
            TempData["GarageError"] = "Preenche marca, modelo e os dados principais do veículo.";
            return RedirectToAction(nameof(Index));
        }

        var createdVehicleId = appState.AddVehicle(userId.Value, input);
        TempData["GarageSuccess"] = "Veículo guardado na garagem.";
        return RedirectToAction(nameof(Index), new { vehicleId = createdVehicleId });
    }

    [HttpPost]
    public IActionResult DeleteVehicle(Guid id)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        appState.RemoveVehicle(userId.Value, id);
        TempData["GarageSuccess"] = "Veículo removido.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult SetDefaultVehicle(Guid id)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        appState.SetDefaultVehicle(userId.Value, id);
        TempData["GarageSuccess"] = "Veículo predefinido atualizado.";
        return RedirectToAction(nameof(Index), new { vehicleId = id });
    }

    [HttpPost]
    public IActionResult UpdateVehicle([Bind(Prefix = "VehicleStatus")] VehicleStatusInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        if (!ModelState.IsValid)
        {
            TempData["GarageError"] = "Não foi possível atualizar os dados do veículo. Verifica os valores introduzidos.";
            return RedirectToAction(nameof(Index), new { vehicleId = input.Id });
        }

        appState.UpdateVehicleGarageData(userId.Value, input);
        TempData["GarageSuccess"] = "Estado do veículo atualizado.";
        return RedirectToAction(nameof(Index), new { vehicleId = input.Id });
    }

    [HttpPost]
    public IActionResult RefuelVehicle([Bind(Prefix = "RefuelInput")] RefuelVehicleInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        if (!ModelState.IsValid || (!input.FillToFull && input.Amount <= 0))
        {
            TempData["GarageError"] = "Indica uma quantidade válida ou seleciona a opção para encher o depósito/bateria.";
            return RedirectToAction(nameof(Index), new { vehicleId = input.Id });
        }

        appState.RefuelVehicle(userId.Value, input);
        TempData["GarageSuccess"] = input.FillToFull
            ? "Veículo marcado como cheio."
            : "Abastecimento/carga registado com sucesso.";
        return RedirectToAction(nameof(Index), new { vehicleId = input.Id });
    }
}
