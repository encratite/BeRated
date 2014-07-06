module BeRated {
	export class Utility {
		static createIcon(className: string) {
			var icon = document.createElement('i');
			icon.classList.add('fa');
			icon.classList.add(className);
			return icon;
		}
	}
}