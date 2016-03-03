module BeRated {
    var cookieName = "timeConstraints";

    function onSelectedTimeConstraint(event: Event) {
        var select = <HTMLSelectElement>this;
        setCookie(cookieName, select.value);
        select.disabled = true;
        location.reload();
    }

    document.addEventListener("DOMContentLoaded", (event) => {
        var select = <HTMLSelectElement>document.querySelector(".menu select");
        if (select == null) {
            return;
        }
        select.onchange = onSelectedTimeConstraint;
        var value = getCookie(cookieName);
        if (value != null) {
            select.value = value;
        }
    });
}