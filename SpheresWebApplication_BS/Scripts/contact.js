$(document).ready(function () {

    function initMap() {
        var location = new google.maps.LatLng(48.837895, 2.495791);
        var mapCanvas = document.getElementById("map-container");
        var mapOptions = {
            center: location,
            zoom: 14,
            panControl: false,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };

        var map = new google.maps.Map(mapCanvas, mapOptions);
        var marker = new google.maps.Marker({
            position: location,
            map: map,
            icon: {
                path: google.maps.SymbolPath.CIRCLE,
                strokeColor: "#C00303",
                scale: 5
            },
            title: "Euro Finance Systems"
        });

        var contentString = '<div id="map-info">' +
                '<img id="mc_logo" src="Images/Logo_Entity/EuroFinanceSystems_Banner.gif" />' +
                '<h3>Euro Finance Systems</h3>' +
                '<p style="color:gray">Provider of Capital Markets Solutions</p>' +
                '</div>';

        var infowindow = new google.maps.InfoWindow({
            content: contentString,
            maxWidth: 400
        });

        marker.addListener('click', function () {
            infowindow.open(map, marker);
        });
        marker.setMap(map);

    }

    google.maps.event.addDomListener(window, 'load', initMap);
});