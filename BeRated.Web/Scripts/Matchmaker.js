var BeRated;
(function (BeRated) {
    var cookieName = "matchmaker";
    var MatchmakerConfiguration = (function () {
        function MatchmakerConfiguration(steamIds, swapTeams) {
            this.steamIds = steamIds;
            this.swapTeams = swapTeams;
        }
        return MatchmakerConfiguration;
    })();
    function getRadioButtons() {
        var selector = ".matchmaker table input[type = \"checkbox\"]";
        var radioButtons = document.querySelectorAll(selector);
        return radioButtons;
    }
    function getSwapTeamsCheckbox() {
        var swapTeamsCheckbox = document.querySelector("#swapTeams");
        return swapTeamsCheckbox;
    }
    function evaluateMatchup(event) {
        var radioButtons = getRadioButtons();
        var steamIds = [];
        for (var i = 0; i < radioButtons.length; i++) {
            var radioButton = radioButtons[i];
            if (radioButton.checked === true) {
                var steamId = radioButton.value;
                steamIds.push(steamId);
            }
        }
        var swapTeamsCheckbox = getSwapTeamsCheckbox();
        var swapTeams = swapTeamsCheckbox.checked === true;
        if (steamIds.length >= 3) {
            var configuration = new MatchmakerConfiguration(steamIds, swapTeams);
            var json = JSON.stringify(configuration);
            BeRated.setCookie(cookieName, json);
            window.location.href = "/Matchmaking?ids=" + steamIds.join() + "&swap=" + swapTeams.toString();
        }
        else {
            alert("You must select at least three players to perform matchmaking.");
        }
    }
    document.addEventListener("DOMContentLoaded", function (event) {
        var button = document.querySelector(".matchmaker input[type = \"button\"]");
        if (button == undefined) {
            return;
        }
        button.onclick = evaluateMatchup;
        var json = BeRated.getCookie(cookieName);
        if (json == null) {
            return;
        }
        var configuration = JSON.parse(json);
        var radioButtons = getRadioButtons();
        for (var i = 0; i < radioButtons.length; i++) {
            var radioButton = radioButtons[i];
            var steamId = radioButton.value;
            if (configuration.steamIds.indexOf(steamId) >= 0) {
                radioButton.checked = true;
            }
        }
        var swapTeamsCheckbox = getSwapTeamsCheckbox();
        swapTeamsCheckbox.checked = configuration.swapTeams;
    });
})(BeRated || (BeRated = {}));
//# sourceMappingURL=Matchmaker.js.map