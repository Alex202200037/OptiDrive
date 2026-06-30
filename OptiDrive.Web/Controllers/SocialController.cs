using Microsoft.AspNetCore.Mvc;
using OptiDrive.Web.Infrastructure;
using OptiDrive.Web.Models;
using OptiDrive.Web.Services;

namespace OptiDrive.Web.Controllers;

public sealed class SocialController(
    AppStateService appState,
    ExternalFuelStationService fuelStations) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        await fuelStations.GetAllStationsAsync(false, cancellationToken);
        var model = appState.BuildDashboard(userId.Value);
        return View(model);
    }

    [HttpGet]
    public IActionResult Pulse()
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var model = appState.BuildDashboard(userId.Value);
        var activeTrip = model.CollaborativeTrips.FirstOrDefault(trip => trip.Status == CollaborativeTripStatus.Active);
        return Json(new
        {
            timestamp = DateTime.UtcNow,
            messages = model.DirectMessages.Take(5).Select(message => new
            {
                sender = message.SenderName,
                body = message.Body,
                sentAt = message.SentAtUtc.ToLocalTime().ToString("g"),
                mine = message.SenderUserId == userId
            }),
            activeTrip = activeTrip is null
                ? null
                : new
                {
                    activeTrip.Name,
                    activeTrip.Origin,
                    activeTrip.Destination,
                    memberCount = activeTrip.Members.Count
                },
            onlineMembers = model.SocialDirectory.Count(member => member.IsOnline)
        });
    }

    [HttpPost]
    public IActionResult UpdateProfile(SocialProfileInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        if (ModelState.IsValid)
        {
            appState.UpdateSocialProfile(userId.Value, input);
            TempData["Success"] = "Perfil social atualizado.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult AddContact(ContactInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        if (ModelState.IsValid)
        {
            appState.AddContact(userId.Value, input);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult RequestAccess(Guid targetUserId)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        TempData["Success"] = appState.RequestSocialAccess(userId.Value, targetUserId)
            ? "Pedido de acesso enviado."
            : "Nao foi possivel enviar o pedido ou ja existe uma ligacao.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult AcceptAccess(Guid requestId)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        TempData["Success"] = appState.AcceptSocialAccess(userId.Value, requestId)
            ? "Pedido aceite. Esta pessoa passou a fazer parte do teu circulo."
            : "Pedido indisponivel.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult DeclineAccess(Guid requestId)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");

        TempData["Success"] = appState.DeclineSocialAccess(userId.Value, requestId)
            ? "Pedido recusado."
            : "Pedido indisponivel.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ShareVehicle(ShareVehicleInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        if (ModelState.IsValid)
        {
            appState.ShareVehicle(userId.Value, input);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult CreateGroup(GroupInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        if (ModelState.IsValid)
        {
            appState.AddGroup(userId.Value, input);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult SendMessage(DirectMessageInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        if (ModelState.IsValid)
        {
            appState.SendDirectMessage(userId.Value, input);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult CreateTrip(CollaborativeTripInputModel input)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        if (ModelState.IsValid)
        {
            appState.CreateCollaborativeTrip(userId.Value, input);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult JoinTrip(Guid id)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        appState.JoinCollaborativeTrip(userId.Value, id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult StartTrip(Guid id)
    {
        var userId = HttpContext.Session.GetCurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Home");
        appState.StartCollaborativeTrip(userId.Value, id);
        TempData["Success"] = "Viagem em grupo iniciada.";
        return RedirectToAction(nameof(Index));
    }
}
