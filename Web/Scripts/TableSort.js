(function () {
	var Table = (function () {
		var type = Table;
		
		var ColumnType = {
			Numeric: 'numeric',
			String: 'string'
		};
		
		function Table(table) {
			this._table = table;
			this._columnTypes = [];
			this._columnIndex = null;
			this._invertOrder = false;
			
			this._initialize();
		}
		
		var proto = type.prototype;
		
		proto.onHeaderClick = function (columnIndex) {
			if (columnIndex === this._columnIndex) {
				this._invertOrder = !this._invertOrder;
			}
			else {
				this._invertOrder = false;
			}
			var headers = this._table.querySelectorAll('th');
			for (var i = 0; i < headers.length; i++) {
				var header = headers[i];
				header.style.textDecoration = 'none';
			}
			var selectedHeader = headers[columnIndex];
			selectedHeader.style.textDecoration = 'underline';
			var body = this._table.querySelector('tbody');
			var allRows = body.querySelectorAll('tr');
			var rows = [];
			for (var i = 1; i < allRows.length; i++) {
				var row = allRows[i];
				rows.push(row);
				body.removeChild(row);
			}
			var columnType = this._columnTypes[columnIndex];
			var getValue = function (row) {
				var cells = row.querySelectorAll('td');
				var cell = cells[columnIndex];
				var value = cell.textContent;
				if (columnType === ColumnType.Numeric) {
					value = parseFloat(value);
				}
				return value;
			};
			rows.sort(function (row1, row2) {
				var value1 = getValue(row1);
				var value2 = getValue(row2);
				var output;
				if (columnType === ColumnType.String) {
					output = value1.localeCompare(value2);
				}
				else {
					if (value1 > value2) {
						output = 1;
					}
					else if (value1 < value2) {
						output = -1;
					}
					else {
						output = 0;
					}
				}
				if (columnType === ColumnType.Numeric) {
					output = - output;
				}
				if (this._invertOrder === true) {
					output = - output;
				}
				return output;
			}.bind(this));
			rows.forEach(function (row) {
				body.appendChild(row);
			});
			this._columnIndex = columnIndex;
		};
		
		proto._initialize = function () {
			var headers = this._table.querySelectorAll('th');
			for (var i = 0; i < headers.length; i++) {
				var header = headers[i];
				var columnType = ColumnType.Numeric;
				for (var key in ColumnType) {
					var type = ColumnType[key];
					if (header.classList.contains(type)) {
						columnType = type;
						break;
					}
				}
				this._columnTypes[i] = columnType;
				header.onmousedown = function (columnIndex) {
					this.onHeaderClick(columnIndex);
				}.bind(this, i);
			}
		};
		
		return type;
	})();
	
	function initializeTables(event) {
		var tables = document.querySelectorAll('table.sortable');
		for (var i = 0; i < tables.length; i++) {
			var table = tables[i];
			new Table(table);
		}
	}
	
	document.addEventListener('DOMContentLoaded', initializeTables);
})();