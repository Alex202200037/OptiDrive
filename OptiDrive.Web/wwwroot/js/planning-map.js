(function () {
    async function loadGoogleMaps(apiKey) {
        if (window.google?.maps?.importLibrary) {
            return;
        }

        await new Promise((resolve, reject) => {
            const script = document.createElement("script");
            script.src = `https://maps.googleapis.com/maps/api/js?key=${encodeURIComponent(apiKey)}&loading=async&libraries=routes,places`;
            script.async = true;
            script.defer = true;
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);
        });
    }

    async function fetchJson(url) {
        const response = await fetch(url, { credentials: "same-origin" });
        if (!response.ok) {
            throw new Error(`Pedido falhou com estado ${response.status}.`);
        }

        return await response.json();
    }

    function stationSupportsFuel(station, fuelKind) {
        if (!fuelKind) {
            return true;
        }

        const available = (station.availableFuelKinds || "")
            .split("|")
            .map((item) => item.trim())
            .filter(Boolean);

        return available.length === 0
            ? station.fuelKind === fuelKind
            : available.includes(fuelKind);
    }

    function stationMatchesBrand(station, brand) {
        return !brand || (station.brand || "").toLowerCase() === brand.toLowerCase();
    }

    function escapeHtml(value) {
        return String(value || "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#39;");
    }

    function fuelLabel(fuelKind) {
        switch (fuelKind) {
            case "Gasoline95": return "Gasolina 95";
            case "Gasoline98": return "Gasolina 98";
            case "Diesel": return "Gasóleo";
            case "GPL": return "GPL";
            case "Electric": return "Elétrico";
            default: return fuelKind || "Combustível";
        }
    }

    function parseFuelPriceMap(station) {
        if (!station.fuelPriceMapJson) {
            return {};
        }

        try {
            return JSON.parse(station.fuelPriceMapJson);
        } catch {
            return {};
        }
    }

    function priceForFuel(station, fuelKind) {
        if (!fuelKind) {
            return station.price || station.selectedFuelPrice || 0;
        }

        const fuelPrices = parseFuelPriceMap(station);
        if (typeof fuelPrices[fuelKind] === "number") {
            return fuelPrices[fuelKind];
        }

        if (typeof station.selectedFuelPrice === "number" && station.selectedFuelPrice > 0) {
            return station.selectedFuelPrice;
        }

        return station.fuelKind === fuelKind ? (station.price || 0) : 0;
    }

    function buildPriceLabel(station, fuelKind) {
        const price = priceForFuel(station, fuelKind);
        if (price > 0) {
            return `${fuelLabel(fuelKind || station.fuelKind)} ${price.toFixed(3)} EUR`;
        }

        return station.fuelPriceSummary || "Preço por confirmar";
    }

    function stationPopupHtml(station, fuelKind) {
        return `
            <strong>${escapeHtml(station.name)}</strong><br>
            ${escapeHtml(station.brand)} · ${escapeHtml(station.city)}<br>
            ${escapeHtml(buildPriceLabel(station, fuelKind))}<br>
            <small>${escapeHtml(station.availableFuels || station.fuelPriceSummary || "")}</small>
        `;
    }

    function stopPopupHtml(stop) {
        const unit = stop.fuelKind === "Electric" ? "kWh" : "L";
        const totalCost = typeof stop.price === "number" && stop.price > 0
            ? `${(stop.price * stop.suggestedAmount).toFixed(2)} EUR`
            : "Custo por confirmar";

        return `
            <strong>Paragem ${stop.id}: ${escapeHtml(stop.stationName)}</strong><br>
            ${escapeHtml(stop.brand || "Sem marca")} · ${escapeHtml(stop.city || "")}<br>
            ${escapeHtml(fuelLabel(stop.fuelKind))} · km ${Number(stop.progressKm || 0).toFixed(0)}<br>
            <small>Repor ${Number(stop.suggestedAmount || 0).toFixed(1)} ${unit} · ${escapeHtml(totalCost)}</small>
        `;
    }

    function renderCostSummary(container, route, selectedFuelKind, suggestedStops) {
        if (!container) {
            return;
        }

        if (!route) {
            container.innerHTML = "";
            return;
        }

        const fuelText = selectedFuelKind ? ` · ${fuelLabel(selectedFuelKind)}` : "";
        const stops = suggestedStops || [];
        const stopSummary = stops.length > 0
            ? `<p>Paragens planeadas ${stops.length} · Próxima aos ${Number(stops[0].progressKm || 0).toFixed(0)} km</p>`
            : "<p>Viagem direta sem paragens intermédias obrigatórias.</p>";
        const routeImpact = typeof route.estimatedConsumptionAmount === "number"
            ? `<p>Consumo estimado ${route.estimatedConsumptionAmount.toFixed(1)} ${selectedFuelKind === "Electric" ? "kWh" : "L"} · Chegada ~${route.estimatedArrivalLevelPercent}%</p>`
            : "";
        const speedImpact = typeof route.adjustedConsumptionPer100 === "number" && route.adjustedConsumptionPer100 > 0
            ? `<p>Consumo a ${Number(route.averageSpeedKmh || 0).toFixed(0)} km/h: ${route.adjustedConsumptionPer100.toFixed(1)} ${selectedFuelKind === "Electric" ? "kWh" : "L"}/100 km · fator ${Number((route.consumptionSpeedFactor || 1) * 100).toFixed(0)}%</p>`
            : "";
        const arrivalReserve = typeof route.minimumArrivalLevelPercent === "number" && route.minimumArrivalLevelPercent > 0
            ? `<p>Margem mínima de chegada ${route.minimumArrivalLevelPercent}%</p>`
            : "";
        const replenishment = typeof route.totalSuggestedReplenishmentAmount === "number" && route.totalSuggestedReplenishmentAmount > 0
            ? `<p>Reforço planeado ${route.totalSuggestedReplenishmentAmount.toFixed(1)} ${selectedFuelKind === "Electric" ? "kWh" : "L"}</p>`
            : "";
        container.innerHTML = `
            <div class="summary-card summary-card--accent">
                <p class="eyebrow">Custo total${escapeHtml(fuelText)}</p>
                <h3>${route.cheapestCost.toFixed(2)} EUR</h3>
                <p>Rápida ${route.fastestCost.toFixed(2)} EUR · Portagens ${route.tollCost.toFixed(2)} EUR</p>
                <p>Distância ${route.distanceKm} km · ETA ${route.etaMinutes} min · Recomendado ${route.recommendedChargeAmount.toFixed(1)}</p>
                ${routeImpact}
                ${speedImpact}
                ${arrivalReserve}
                ${replenishment}
                ${stopSummary}
            </div>
        `;
    }

    function renderDirectionsFallback(container, route, suggestedStops) {
        if (!route) {
            container.innerHTML = "<h3>Sem rota recente</h3><p>Cria uma rota para ver instruções, custo e postos recomendados.</p>";
            return;
        }

        const stopMarkup = (suggestedStops || []).length > 0
            ? `
                <div class="route-stop-stack route-stop-stack--map">
                    ${(suggestedStops || []).slice(0, 4).map((stop) => `
                        <div class="route-stop-card route-stop-card--compact">
                            <span class="route-stop-index">Paragem ${stop.id}</span>
                            <strong>${escapeHtml(stop.stationName)}</strong>
                            <p>${escapeHtml(stop.brand || "")} · km ${Number(stop.progressKm || 0).toFixed(0)} · ${Number(stop.suggestedAmount || 0).toFixed(1)} ${stop.fuelKind === "Electric" ? "kWh" : "L"}</p>
                        </div>
                    `).join("")}
                </div>
            `
            : "";

        container.innerHTML = `
            <h3>${escapeHtml(route.origin)} → ${escapeHtml(route.destination)}</h3>
            <p>${route.distanceKm} km · ETA ${route.etaMinutes} min · Portagens ${route.tollCost.toFixed(2)} EUR</p>
            <p>Rápida ${route.fastestCost.toFixed(2)} EUR · Económica ${route.cheapestCost.toFixed(2)} EUR · Score ${route.savingsScore}/10</p>
            <p>Consumo estimado ${Number(route.estimatedConsumptionAmount || 0).toFixed(1)} ${selectedFuelKindFromRoute(route, suggestedStops)} · Chegada ~${Number(route.estimatedArrivalLevelPercent || 0).toFixed(0)}% · margem alvo ${Number(route.minimumArrivalLevelPercent || 0).toFixed(0)}%</p>
            <p>Média ${Number(route.averageSpeedKmh || 0).toFixed(0)} km/h · consumo ajustado ${Number(route.adjustedConsumptionPer100 || 0).toFixed(1)} ${selectedFuelKindFromRoute(route, suggestedStops)}/100 km</p>
            <p>${escapeHtml(route.recommendation)}</p>
            <p>${escapeHtml(route.criticalAlert)}</p>
            ${stopMarkup}
        `;
    }

    function selectedFuelKindFromRoute(route, suggestedStops) {
        if ((suggestedStops || []).some((stop) => stop.fuelKind === "Electric")) {
            return "kWh";
        }

        return "L";
    }

    function setActiveFuelChip(fuelKind) {
        const buttons = Array.from(document.querySelectorAll("[data-fuel-filter]"));
        let matched = false;

        buttons.forEach((button) => {
            const active = (button.dataset.fuelFilter || "") === (fuelKind || "");
            button.classList.toggle("active", active);
            button.setAttribute("aria-pressed", String(active));
            matched = matched || active;
        });

        if (!matched) {
            const allButton = buttons.find((button) => (button.dataset.fuelFilter || "") === "");
            if (allButton) {
                allButton.classList.add("active");
                allButton.setAttribute("aria-pressed", "true");
            }
        }
    }

    function selectedFuelFromVehicle() {
        const vehicleSelect = document.querySelector("#route-vehicle");
        const option = vehicleSelect?.selectedOptions?.[0];
        return option?.dataset.fuelKind || "";
    }

    function setupFilters(payload, applyFilter, initialFuel) {
        const fuelFilterButtons = Array.from(document.querySelectorAll("[data-fuel-filter]"));
        const brandFilter = document.querySelector("#brand-filter");
        const vehicleSelect = document.querySelector("#route-vehicle");
        const state = {
            fuel: initialFuel || "",
            brand: ""
        };

        if (brandFilter) {
            brandFilter.innerHTML = `<option value="">Todas as marcas</option>${(payload.brands || [])
                .map((brand) => `<option value="${escapeHtml(brand)}">${escapeHtml(brand)}</option>`)
                .join("")}`;

            brandFilter.addEventListener("change", () => {
                state.brand = brandFilter.value || "";
                applyFilter(state.fuel, state.brand);
            });
        }

        fuelFilterButtons.forEach((button) => {
            button.addEventListener("click", () => {
                state.fuel = button.dataset.fuelFilter || "";
                setActiveFuelChip(state.fuel);
                applyFilter(state.fuel, state.brand);
            });
        });

        if (vehicleSelect) {
            vehicleSelect.addEventListener("change", () => {
                state.fuel = selectedFuelFromVehicle();
                setActiveFuelChip(state.fuel);
                applyFilter(state.fuel, state.brand);
            });
        }

        setActiveFuelChip(state.fuel);
        applyFilter(state.fuel, state.brand);
        return state;
    }

    function markerColor(station) {
        if (station.isElectricCharging) {
            return "#2a72d4";
        }

        return station.isLowCost ? "#0d6b55" : "#d2a531";
    }

    async function renderGoogleMap(container, costSummary, panel, payload, initialFuel) {
        const apiKey = container.dataset.googleKey;
        await loadGoogleMaps(apiKey);

        const { Map, InfoWindow } = await google.maps.importLibrary("maps");
        const { DirectionsService, DirectionsRenderer } = await google.maps.importLibrary("routes");

        const map = new Map(container, {
            center: { lat: 38.7223, lng: -9.1393 },
            zoom: 7,
            mapId: "OPTIDRIVE_MAP"
        });

        const infoWindow = new InfoWindow();
        const filterState = { fuel: initialFuel || "" };
        let visibleMarkers = [];
        let suggestedStopMarkers = [];

        function drawSuggestedStops() {
            suggestedStopMarkers.forEach((marker) => marker.setMap(null));
            suggestedStopMarkers = (payload.suggestedStops || []).map((stop) => {
                const marker = new google.maps.Marker({
                    position: { lat: stop.latitude, lng: stop.longitude },
                    map,
                    title: `Paragem ${stop.id}: ${stop.stationName}`,
                    icon: {
                        path: google.maps.SymbolPath.CIRCLE,
                        scale: 9,
                        fillColor: "#d04b36",
                        fillOpacity: 1,
                        strokeColor: "#ffffff",
                        strokeWeight: 2
                    },
                    label: {
                        text: String(stop.id),
                        color: "#ffffff",
                        fontSize: "11px",
                        fontWeight: "700"
                    },
                    zIndex: 1000
                });

                marker.addListener("click", () => {
                    infoWindow.setContent(stopPopupHtml(stop));
                    infoWindow.open({ anchor: marker, map });
                });

                return marker;
            });
        }

        function drawStations(fuelKind, brand) {
            filterState.fuel = fuelKind || "";
            visibleMarkers.forEach((marker) => marker.setMap(null));

            visibleMarkers = payload.stations
                .filter((station) => stationSupportsFuel(station, fuelKind) && stationMatchesBrand(station, brand))
                .slice(0, 1600)
                .map((station) => {
                    const marker = new google.maps.Marker({
                        position: { lat: station.latitude, lng: station.longitude },
                        map,
                        title: `${station.name} · ${station.brand}`,
                        icon: {
                            path: google.maps.SymbolPath.CIRCLE,
                            scale: station.isElectricCharging ? 6 : (station.isLowCost ? 5 : 4),
                            fillColor: markerColor(station),
                            fillOpacity: 0.95,
                            strokeColor: "#ffffff",
                            strokeWeight: 1
                        }
                    });

                    marker.addListener("click", () => {
                        infoWindow.setContent(stationPopupHtml(station, filterState.fuel));
                        infoWindow.open({ anchor: marker, map });
                    });
                    return marker;
                });

            drawSuggestedStops();
            renderCostSummary(costSummary, payload.route, filterState.fuel, payload.suggestedStops);
        }

        setupFilters(payload, drawStations, initialFuel);

        if (!payload.route || !payload.routePath?.length) {
            renderDirectionsFallback(panel, payload.route, payload.suggestedStops);
            renderCostSummary(costSummary, payload.route, filterState.fuel, payload.suggestedStops);
            return;
        }

        const routeLatLngs = payload.routePath.map((point) => ({ lat: point.latitude, lng: point.longitude }));
        const routeBounds = new google.maps.LatLngBounds();
        routeLatLngs.forEach((point) => routeBounds.extend(point));

        new google.maps.Polyline({
            path: routeLatLngs,
            geodesic: true,
            strokeColor: "#0b5e47",
            strokeOpacity: 0.92,
            strokeWeight: 5,
            map
        });

        map.fitBounds(routeBounds, 32);

        const directionsService = new DirectionsService();
        const directionsRenderer = new DirectionsRenderer({
            map: null,
            panel,
            suppressMarkers: true,
            preserveViewport: false
        });

        drawSuggestedStops();
        renderCostSummary(costSummary, payload.route, filterState.fuel, payload.suggestedStops);

        const waypointList = String(payload.route.waypoints || "")
            .split(",")
            .map((item) => item.trim())
            .filter(Boolean)
            .map((location) => ({ location, stopover: true }));

        await directionsService.route({
            origin: payload.route.origin,
            destination: payload.route.destination,
            waypoints: waypointList,
            travelMode: google.maps.TravelMode.DRIVING,
            avoidTolls: payload.route.avoidTolls === true,
            provideRouteAlternatives: false
        }).then((result) => {
            directionsRenderer.setDirections(result);
        }).catch(() => {
            renderDirectionsFallback(panel, payload.route, payload.suggestedStops);
        });
    }

    function renderLeafletMap(container, costSummary, panel, payload, initialFuel) {
        const map = L.map(container).setView([38.7223, -9.1393], 7);
        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            attribution: "&copy; OpenStreetMap contributors"
        }).addTo(map);

        const allStations = payload.stations || [];
        const state = { fuel: initialFuel || "" };
        let stationLayer = L.layerGroup().addTo(map);
        let stopLayer = L.layerGroup().addTo(map);

        function drawStations(fuelKind, brand) {
            state.fuel = fuelKind || "";
            stationLayer.clearLayers();
            const filtered = allStations.filter((station) => stationSupportsFuel(station, fuelKind) && stationMatchesBrand(station, brand));
            filtered.slice(0, 2500).forEach((station) => {
                const marker = L.circleMarker([station.latitude, station.longitude], {
                    radius: station.isElectricCharging ? 6.5 : (station.isLowCost ? 5.5 : 4.5),
                    color: markerColor(station),
                    weight: 1,
                    fillOpacity: 0.8
                });

                marker.bindPopup(stationPopupHtml(station, state.fuel));
                marker.addTo(stationLayer);
            });

            stopLayer.clearLayers();
            (payload.suggestedStops || []).forEach((stop) => {
                const marker = L.circleMarker([stop.latitude, stop.longitude], {
                    radius: 8,
                    color: "#d04b36",
                    weight: 2,
                    fillOpacity: 0.95
                });

                marker.bindPopup(stopPopupHtml(stop));
                marker.addTo(stopLayer);
            });

            renderCostSummary(costSummary, payload.route, state.fuel, payload.suggestedStops);
        }

        setupFilters(payload, drawStations, initialFuel);

        if (payload.routePath?.length) {
            const latLngs = payload.routePath.map((point) => [point.latitude, point.longitude]);
            L.polyline(latLngs, { color: "#0b5e47", weight: 5, opacity: 0.85 }).addTo(map);
            map.fitBounds(latLngs, { padding: [24, 24] });
        }

        renderDirectionsFallback(panel, payload.route, payload.suggestedStops);
        renderCostSummary(costSummary, payload.route, state.fuel, payload.suggestedStops);
    }

    async function setupPlaceSearch() {
        const form = document.querySelector("form[data-place-search-url]");
        if (!form) {
            return;
        }

        const endpoint = form.getAttribute("data-place-search-url");
        const inputs = Array.from(form.querySelectorAll(".place-search-input"));
        const apiKey = document.querySelector("#planner-map")?.dataset.googleKey || "";

        function wireServerSuggestions(input) {
            const suggestionBox = input.parentElement?.querySelector(".place-suggestions");
            if (!suggestionBox || !endpoint) {
                return;
            }

            let lastToken = 0;
            input.addEventListener("input", async () => {
                const query = input.value.trim();
                if (query.length < 3) {
                    suggestionBox.innerHTML = "";
                    return;
                }

                const token = ++lastToken;
                try {
                    const payload = await fetchJson(`${endpoint}?q=${encodeURIComponent(query)}`);
                    if (token !== lastToken) {
                        return;
                    }

                    const items = payload.results || [];
                    suggestionBox.innerHTML = items.map((item) => `
                        <button type="button" class="place-suggestion-item" data-value="${escapeHtml(item.value)}">
                            <span>${escapeHtml(item.label)}</span>
                            <small>${escapeHtml(item.provider)}</small>
                        </button>
                    `).join("");

                    suggestionBox.querySelectorAll("[data-value]").forEach((button) => {
                        button.addEventListener("click", () => {
                            input.value = button.getAttribute("data-value") || "";
                            suggestionBox.innerHTML = "";
                        });
                    });
                } catch {
                    suggestionBox.innerHTML = "";
                }
            });

            input.addEventListener("blur", () => {
                window.setTimeout(() => {
                    suggestionBox.innerHTML = "";
                }, 150);
            });
        }

        inputs.forEach(wireServerSuggestions);

        if (!apiKey) {
            return;
        }

        try {
            await loadGoogleMaps(apiKey);
            const { Autocomplete } = await google.maps.importLibrary("places");

            inputs.forEach((input) => {
                const autocomplete = new Autocomplete(input, {
                    componentRestrictions: { country: ["pt"] },
                    fields: ["formatted_address", "geometry", "name"],
                    types: ["geocode"]
                });

                autocomplete.addListener("place_changed", () => {
                    const place = autocomplete.getPlace();
                    if (place?.formatted_address) {
                        input.value = place.formatted_address;
                    }
                });
            });
        } catch {
        }
    }

    document.addEventListener("DOMContentLoaded", async () => {
        await setupPlaceSearch();

        const mapContainer = document.querySelector("#planner-map");
        const costSummaryContainer = document.querySelector("#planner-cost-summary");
        const directionsContainer = document.querySelector("#planner-directions");

        if (!mapContainer || !directionsContainer) {
            return;
        }

        const endpoint = mapContainer.dataset.mapEndpoint;
        const routeId = mapContainer.dataset.routeId;
        const url = routeId ? `${endpoint}?routeId=${encodeURIComponent(routeId)}` : endpoint;
        let payload;
        try {
            payload = await fetchJson(url);
        } catch {
            mapContainer.innerHTML = "<div class='map-fallback-message'><strong>Mapa temporariamente indisponível.</strong><span>O planeamento continua acessível; tenta atualizar dentro de momentos.</span></div>";
            directionsContainer.innerHTML = "<h3>Não foi possível carregar os dados do mapa.</h3><p>Confirma a ligação e volta a tentar.</p>";
            return;
        }

        const apiKey = mapContainer.dataset.googleKey;
        const initialFuel = selectedFuelFromVehicle() || payload.selectedVehicle?.fuelKind || "";

        try {
            if (apiKey) {
                await renderGoogleMap(mapContainer, costSummaryContainer, directionsContainer, payload, initialFuel);
            } else {
                renderLeafletMap(mapContainer, costSummaryContainer, directionsContainer, payload, initialFuel);
            }
        } catch {
            try {
                renderLeafletMap(mapContainer, costSummaryContainer, directionsContainer, payload, initialFuel);
            } catch {
                mapContainer.innerHTML = "<div class='map-fallback-message'><strong>Mapa temporariamente indisponível.</strong><span>A rota e os custos permanecem visíveis nesta página.</span></div>";
                renderDirectionsFallback(directionsContainer, payload.route, payload.suggestedStops);
                renderCostSummary(costSummaryContainer, payload.route, initialFuel, payload.suggestedStops);
            }
        }
    });
})();
