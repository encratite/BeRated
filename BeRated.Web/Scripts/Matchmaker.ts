module BeRated {
    function evaluateMatchup(event: MouseEvent) {
        var radioButtons = <NodeListOf<HTMLInputElement>>document.querySelectorAll(".matchmaker input[type = \"checkbox\"]");
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

    document.addEventListener("DOMContentLoaded", (event) => {
        var button = <HTMLInputElement>document.querySelector(".matchmaker input[type = \"button\"]");
        if (button != undefined) {
            button.onclick = evaluateMatchup;
        }
    });
}