module BeRated {
    class Table {
        private static ColumnType = {
            Numeric: "numeric",
            String: "string"
        };

        private table: HTMLTableElement;
        private columnTypes: string[];
        private columnIndex: number;
        private invertOrder: boolean;

        constructor(table: HTMLTableElement) {
            this.table = table;
            this.columnTypes = [];
            this.columnIndex = null;
            this.invertOrder = false;

            this.initialize();
        }

        private onHeaderClick(columnIndex: number) {
            if (columnIndex === this.columnIndex) {
                this.invertOrder = !this.invertOrder;
            }
            else {
                this.invertOrder = false;
            }
            var headers = <NodeListOf<HTMLElement>>this.table.querySelectorAll("th");
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
            var getValue = (row) => {
                var cells = row.querySelectorAll("td");
                var cell = cells[columnIndex];
                var value = cell.textContent;
                if (columnType === Table.ColumnType.Numeric) {
                    value = parseFloat(value);
                }
                return value;
            };
            rows.sort((row1, row2) => {
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
                    output = - output;
                }
                if (this.invertOrder === true) {
                    output = - output;
                }
                return output;
            });
            rows.forEach((row) => {
                body.appendChild(row);
            });
            this.columnIndex = columnIndex;
        }

        private initialize() {
            var headers = <NodeListOf<HTMLElement>>this.table.querySelectorAll("th");
            for (var i = 0; i < headers.length; i++) {
                var header = headers[i];
                var columnType = Table.ColumnType.Numeric;
                for (var key in Table.ColumnType) {
                    var type = Table.ColumnType[key];
                    var dataColumnType = header.dataset["columnType"];
                    if (dataColumnType != undefined) {
                        columnType = dataColumnType;
                        break;
                    }
                }
                this.columnTypes[i] = columnType;
                header.onmousedown = function (columnIndex) {
                    this.onHeaderClick(columnIndex);
                }.bind(this, i);
            }
        }
    }

    function initializeSortableTables() {
        var tables = <NodeListOf<HTMLTableElement>>document.querySelectorAll("table.sortable");
        for (var i = 0; i < tables.length; i++) {
            var table = tables[i];
            var rows = table.querySelectorAll("tr");
            new Table(table);
        }
    }

    function adjustEmptyTables() {
        var tables = document.querySelectorAll("table.grid");
        for (var i = 0; i < tables.length; i++) {
            var table = tables[i];
            var rows = table.querySelectorAll("tr");
            if (rows.length > 1) {
                continue;
            }
            var body = table.querySelector("tbody");
            var columns = table.querySelectorAll("th");
            var row = document.createElement("tr");
            row.className = "noData";
            body.appendChild(row);
            var cell = document.createElement("td");
            cell.textContent = "No data available";
            cell.colSpan = columns.length;
            row.appendChild(cell);
        }
    }

    document.addEventListener("DOMContentLoaded", (event) => {
        initializeSortableTables();
        adjustEmptyTables();
    });
}