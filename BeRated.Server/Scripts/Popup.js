(function () {
	var None = 'none';
	var Block = 'inline-block';
	
	function setPopupVisibility(visible) {
		var popup = getPopup();
		popup.style.display = visible === true ? Block : None;
	}
		
	function getPopup() {
		var popup = document.getElementById('frameworkPopup');
		return popup;
	}
	
	function onSelectorClick(event) {
		var popup = getPopup();
		var visible = popup.style.display !== Block;
		setPopupVisibility(visible);
		event.stopPropagation();
	}
	
	function onClick(event) {
		setPopupVisibility(false);
	}
	
	function initializePopup() {
		var selector = document.getElementById('frameworkSelector');
        selector.onclick = onSelectorClick;
	}
	
	document.addEventListener('DOMContentLoaded', initializePopup);
	document.addEventListener('click', onClick);
})();