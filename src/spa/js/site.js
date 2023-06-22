// Set the base URL for the API
var baseApiUrl = "https://host.com";

// Set home centre point (Microsoft San Francisco office) and starting range
var startLat = 37.7919346;
var startLon = -122.4052692;
var startRange = 800;

// Hook up the enter key to load trucks
document.querySelectorAll('input[type=text]').forEach(item => {
    item.addEventListener('keyup', event => {
        if (event.code === "Enter") {
            loadTrucks();
        }
    })
})

document.getElementById("reload").addEventListener("click", function () {
    loadTrucks();
});

// Define truck icon
var truckIcon = L.icon({
    iconUrl: 'img/truck.png',
    iconSize: [64, 37],
    popupAnchor: [0, -20]
});

// Add a variable to hold the geo layer
var geoLayer;

// Create the map
var map = L.map('map').setView([startLat, startLon], 16);

// Set the start point in the text boxes
document.getElementById("lon").value = startLon;
document.getElementById("lat").value = startLat;
document.getElementById("range").value = startRange;

// Add OSM tile layer
var tiles = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);

// Add pop-up to each item
function onEachFeature(feature, layer) {

    var popupContent = "";

    if (feature.properties) {
        popupContent = '<p>' + feature.properties.address + '</p>';
        popupContent += '<strong>' + feature.properties.applicant + '</strong></br>';
        popupContent += feature.properties.fooditems;
    }

    layer.bindPopup(popupContent);
}

// Show all trucks with a specified range
function loadTrucks() {

    // Show the loading overlay
    document.getElementById("container").style.display = 'flex';

    // Get the values from the inputs
    var lon = document.getElementById("lon").value;
    var lat = document.getElementById("lat").value;
    var range = document.getElementById("range").value;

    // Remove the existing geo layer
    if (geoLayer != undefined) {
        map.removeLayer(geoLayer);
    }

    // Create a new geo layer
    geoLayer = L.geoJSON().addTo(map);

    // Call the API to get truck data with our specified values
    fetch(baseApiUrl + '/api/GetTrucks?lon=' + lon + '&lat=' + lat + '&range=' + range)
        .then(response => response.json())
        .then(body => {

            // Draw the range on the geo layer
            L.circle([lat, lon], {
                color: 'green',
                fillColor: '#0f3',
                fillOpacity: 0.2,
                radius: range
            }).addTo(geoLayer);

            // Add the trucks to the geo layer
            L.geoJSON(body, {
                pointToLayer: function (feature, latlng) {
                    return L.marker(latlng, { icon: truckIcon });
                },
                onEachFeature: onEachFeature
            }).addTo(geoLayer);

            // Check to make sure we have a geo layer and if so set the visible bounds of the map
            if (geoLayer.getLayers() && geoLayer.getLayers().length > 1) {
                map.fitBounds(geoLayer.getBounds());
            }
            else {
                map.panTo([lat, lon], 16, { animation: true });
            }

            // Remove the loading spinner
            document.getElementById("container").style.display = 'none';

        });
}

// Load the trucks
loadTrucks();
