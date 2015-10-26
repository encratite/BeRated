(function () {
	function initializeLinks() {
		var elements = document.querySelectorAll("span[data-days]");
		for (var i = 0; i < elements.length; i++) {
			var element = elements[i];
			element.onclick = onLinkClick;
		}
	}

	function onLinkClick(event) {
		var days = this.getAttribute("data-days");
		document.cookie = "days=" + days + ";";
		location.reload();
	}
	
	document.addEventListener('DOMContentLoaded', initializeLinks);
})();