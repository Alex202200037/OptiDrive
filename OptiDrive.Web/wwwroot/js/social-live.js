(() => {
  const liveCard = document.querySelector("[data-social-pulse-url]");
  if (!liveCard) {
    return;
  }

  const pulseUrl = liveCard.dataset.socialPulseUrl;
  const status = document.getElementById("socialPulseStatus");
  const activeTripName = document.getElementById("activeTripName");
  const activeTripRoute = document.getElementById("activeTripRoute");
  const activeTripMembers = document.getElementById("activeTripMembers");
  const messages = document.getElementById("socialPulseMessages");

  const renderMessages = (items) => {
    if (!messages || !Array.isArray(items)) {
      return;
    }

    messages.innerHTML = items.map((item) => `
      <div class="message-bubble ${item.mine ? "message-bubble--mine" : ""}">
        <strong>${escapeHtml(item.sender)}</strong>
        <p>${escapeHtml(item.body)}</p>
        <small>${escapeHtml(item.sentAt)}</small>
      </div>
    `).join("");
  };

  const refresh = async () => {
    try {
      const response = await fetch(pulseUrl, { headers: { "Accept": "application/json" } });
      if (!response.ok) {
        return;
      }

      const data = await response.json();
      if (status) {
        status.textContent = `${data.onlineMembers ?? 0} online`;
      }

      if (data.activeTrip && activeTripName) {
        activeTripName.textContent = data.activeTrip.name;
        if (activeTripRoute) {
          activeTripRoute.textContent = `${data.activeTrip.origin} → ${data.activeTrip.destination}`;
        }
        if (activeTripMembers) {
          activeTripMembers.textContent = `${data.activeTrip.memberCount} pessoas`;
        }
      }

      renderMessages(data.messages);
    } catch {
      // If the server is restarting, keep the current content intact.
    }
  };

  const escapeHtml = (value) => String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");

  window.setTimeout(refresh, 1500);
  window.setInterval(refresh, 15000);
})();
