module BeRated {
    export function getCookie(name: string): string {
        var pattern = new RegExp("\\b" + name + "=([^;]*)");
        var match = pattern.exec(document.cookie);
        if (match == null) {
            return;
        }
        var value = match[1];
        return value;
    }

    export function setCookie(name: string, value: string) {
        var days = 30;
        var now = new Date();
        var millisecondsPerDay = 24 * 60 * 60 * 1000;
        var timestamp = now.getTime() + days * millisecondsPerDay;
        var expirationDate = new Date(timestamp);
        document.cookie = name + "=" + value + "; expires=" + expirationDate.toUTCString() + "; path=/";
    }
}