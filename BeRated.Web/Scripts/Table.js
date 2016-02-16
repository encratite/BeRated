var BeRated;
(function (BeRated) {
    var Table = (function () {
        function Table(table) {
            this.table = table;
            this.columnTypes = [];
            this.columnIndex = null;
            this.invertOrder = false;
            this.initialize();
        }
        Table.prototype.onHeaderClick = function (columnIndex) {
            var _this = this;
            if (columnIndex === this.columnIndex) {
                this.invertOrder = !this.invertOrder;
            }
            else {
                this.invertOrder = false;
            }
            var headers = this.table.querySelectorAll("th");
            for (var i = 0; i < headers.length; i++) {
                var header = headers[i];
                header.style.textDecoration = "none";
            }
            var selectedHeader = headers[columnIndex];
            selectedHeader.style.textDecoration = "underline";
            var body = this.table.querySelector("tbody");
            var allRows = body.querySelectorAll("tr");
            var rows = [];
            for (var i = 1; i < allRows.length; i++) {
                var row = allRows[i];
                rows.push(row);
                body.removeChild(row);
            }
            var columnType = this.columnTypes[columnIndex];
            var getValue = function (row) {
                var cells = row.querySelectorAll("td");
                var cell = cells[columnIndex];
                var value = cell.textContent;
                if (columnType === Table.ColumnType.Numeric) {
                    value = parseFloat(value);
                }
                return value;
            };
            rows.sort(function (row1, row2) {
                var value1 = getValue(row1);
                var value2 = getValue(row2);
                var output;
                if (columnType === Table.ColumnType.String) {
                    output = value1.localeCompare(value2);
                }
                else {
                    if (value1 === value2) {
                        output = 0;
                    }
                    else if (isNaN(value2) || value1 > value2) {
                        output = 1;
                    }
                    else {
                        output = -1;
                    }
                }
                if (columnType === Table.ColumnType.Numeric) {
                    output = -output;
                }
                if (_this.invertOrder === true) {
                    output = -output;
                }
                return output;
            });
            rows.forEach(function (row) {
                body.appendChild(row);
            });
            this.columnIndex = columnIndex;
        };
        Table.prototype.initialize = function () {
            var headers = this.table.querySelectorAll("th");
            for (var i = 0; i < headers.length; i++) {
                var header = headers[i];
                var columnType = Table.ColumnType.Numeric;
                for (var key in Table.ColumnType) {
                    var type = Table.ColumnType[key];
                    if (header.classList.contains(type)) {
                        columnType = type;
                        break;
                    }
                }
                this.columnTypes[i] = columnType;
                header.onmousedown = function (columnIndex) {
                    this.onHeaderClick(columnIndex);
                }.bind(this, i);
            }
        };
        Table.ColumnType = {
            Numeric: "numeric",
            String: "string"
        };
        return Table;
    })();
    document.addEventListener("DOMContentLoaded", function (event) {
        var tables = document.querySelectorAll("table.sortable");
        for (var i = 0; i < tables.length; i++) {
            var table = tables[i];
            new Table(table);
        }
    });
})(BeRated || (BeRated = {}));
//# sourceMappingURL=Table.js.map