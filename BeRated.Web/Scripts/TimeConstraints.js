var BeRated;
(function (BeRated) {
    var cookieName = "timeConstraints";
    function onSelectedTimeConstraint(event) {
        var select = this;
        BeRated.setCookie(cookieName, select.value);
        select.disabled = true;
        location.reload();
    }
    document.addEventListener("DOMContentLoaded", function (event) {
        var select = document.querySelector(".menu select");
        if (select == null) {
            return;
        }
        select.onchange = onSelectedTimeConstraint;
        var value = BeRated.getCookie(cookieName);
        if (value != null) {
            select.value = value;
        }
    });
})(BeRated || (BeRated = {}));
//# sourceMappingURL=TimeConstraints.js.map