module BeRated {
    var cookieName = "timeConstraints";

    function onSelectedTimeConstraint(event: Event) {
        var select = <HTMLSelectElement>this;
        var days = 30;
        var now = new Date();
        var millisecondsPerDay = 24 * 60 * 60 * 1000;
        var timestamp = now.getTime() + days * millisecondsPerDay;
        var expirationDate = new Date(timestamp);
        console.log(expirationDate);
        document.cookie = cookieName + "=" + select.value + "; expires=" + expirationDate.getUTCDate() + "; path=/";
        select.disabled = true;
        location.reload();
    }

    document.addEventListener("DOMContentLoaded", (event) => {
        var select = <HTMLSelectElement>document.querySelector(".menu select");
        if (select == null) {
            return;
        }
        select.onchange = onSelectedTimeConstraint;
        var pattern = new RegExp("\\b" + cookieName + "=([^;]*)");
        var match = pattern.exec(document.cookie);
        if (match == null) {
            return;
        }
        var value = match[1];
        select.value = value;
    });
}