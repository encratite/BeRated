var BeRated;
(function (BeRated) {
    function evaluateMatchup(event) {
        var radioButtons = document.querySelectorAll(".matchmaker input[type = \"checkbox\"]");
        var steamIds = [];
        for (var i = 0; i < radioButtons.length; i++) {
            var radioButton = radioButtons[i];
            if (radioButton.checked === true) {
                var steamId = radioButton.value;
                steamIds.push(steamId);
            }
        }
        if (steamIds.length >= 3) {
            window.location.href = "/Matchmaking?ids=" + steamIds.join();
        }
        else {
            alert("Not enough players selected.");
        }
    }
    document.addEventListener("DOMContentLoaded", function (event) {
        var button = document.querySelector(".matchmaker input[type = \"button\"]");
        if (button != undefined) {
            button.onclick = evaluateMatchup;
        }
    });
})(BeRated || (BeRated = {}));
//# sourceMappingURL=Matchmaker.js.map