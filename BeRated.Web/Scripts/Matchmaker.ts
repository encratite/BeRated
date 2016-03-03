module BeRated {
    var cookieName = "matchmaker";

    class MatchmakerConfiguration {
        steamIds: string[];
        swapTeams: boolean;

        constructor(steamIds: string[], swapTeams: boolean) {
            this.steamIds = steamIds;
            this.swapTeams = swapTeams;
        }
    }

    function getRadioButtons(): NodeListOf<HTMLInputElement> {
        var selector = ".matchmaker table input[type = \"checkbox\"]";
        var radioButtons = <NodeListOf<HTMLInputElement>>document.querySelectorAll(selector);
        return radioButtons;
    }

    function getSwapTeamsCheckbox(): HTMLInputElement {
        var swapTeamsCheckbox = <HTMLInputElement>document.querySelector("#swapTeams");
        return swapTeamsCheckbox;
    }

    function evaluateMatchup(event: MouseEvent) {
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
            setCookie(cookieName, json);
            window.location.href = "/Matchmaking?ids=" + steamIds.join() + "&swap=" + swapTeams.toString();
        }
        else {
            alert("You must select at least three players to perform matchmaking.");
        }
    }

    document.addEventListener("DOMContentLoaded", (event) => {
        var button = <HTMLInputElement>document.querySelector(".matchmaker input[type = \"button\"]");
        if (button == undefined) {
            return;
        }
        button.onclick = evaluateMatchup;
        var json = getCookie(cookieName);
        if (json == null) {
            return;
        }
        var configuration = <MatchmakerConfiguration>JSON.parse(json);
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
}