module BeRated {
	export class Route {
		pattern: RegExp;
		handler: Function;

		constructor(pattern: RegExp, handler: Function) {
			this.pattern = pattern;
			this.handler = handler;
		}

		match(path: string): boolean {
			var match = this.pattern.exec(path);
			if (match == null)
				return false;
			var captures: Array<string> = [];
			for (var i = 1; i < match.length; i++) {
				var capture = match[i];
				captures.push(capture);
			}
			this.handler.apply(null, captures);
			return true;
		}
	}
} 