var BeRated;
(function (BeRated) {
    var RatingChart = (function () {
        function RatingChart(container) {
            this.container = container;
            this.configuration = JSON.parse(container.textContent);
            container.removeChild(container.firstChild);
        }
        RatingChart.prototype.loadChartData = function () {
            var _this = this;
            var request = new XMLHttpRequest();
            request.onreadystatechange = function (event) {
                if (request.readyState === XMLHttpRequest.DONE) {
                    var ratings = JSON.parse(request.response);
                    _this.createChart(ratings);
                }
            };
            var requestPath = this.configuration.controller + window.location.search;
            request.open("GET", requestPath);
            request.send();
        };
        RatingChart.prototype.createChart = function (ratings) {
            var _this = this;
            var series = [];
            this.configuration.series.forEach(function (seriesConfiguration) {
                ratings.forEach(function (playerRatings) {
                    var seriesName = seriesConfiguration.name != null ? seriesConfiguration.name : playerRatings.name;
                    var serverData = playerRatings[seriesConfiguration.property];
                    var seriesData = _this.getSeriesData(serverData);
                    var seriesObject = {
                        name: seriesName,
                        data: seriesData
                    };
                    series.push(seriesObject);
                });
            });
            var chart = new Highcharts.Chart({
                chart: {
                    renderTo: this.container,
                    zoomType: "x"
                },
                credits: {
                    enabled: false
                },
                title: {
                    text: "<b>" + this.configuration.title + "</b>"
                },
                legend: {
                    enabled: this.configuration.enableLegend || false
                },
                xAxis: {
                    type: "datetime",
                    title: {
                        text: "Time"
                    },
                    labels: {
                        format: "{value:%b %Y}",
                        y: 30
                    }
                },
                yAxis: {
                    title: {
                        text: "Rating"
                    }
                },
                tooltip: {
                    headerFormat: "<b>{series.name}</b><br>",
                    pointFormat: "{point.x:%Y-%m-%d %H:%M:%S}: {point.y:,.1f}"
                },
                series: series
            });
        };
        RatingChart.prototype.getSeriesData = function (samples) {
            return samples.map(function (sample) { return [
                Date.parse(sample.time),
                sample.value
            ]; });
        };
        return RatingChart;
    })();
    document.addEventListener("DOMContentLoaded", function (event) {
        var containers = document.querySelectorAll("div.ratingChart");
        for (var i = 0; i < containers.length; i++) {
            var container = containers[i];
            var chart = new RatingChart(container);
            chart.loadChartData();
        }
    });
})(BeRated || (BeRated = {}));
//# sourceMappingURL=RatingChart.js.map