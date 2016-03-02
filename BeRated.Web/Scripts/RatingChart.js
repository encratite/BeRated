var BeRated;
(function (BeRated) {
    var RatingChart = (function () {
        function RatingChart() {
            this.ratingChartId = "ratingChart";
        }
        RatingChart.prototype.loadChartData = function () {
            var _this = this;
            var chartElement = document.getElementById(this.ratingChartId);
            if (chartElement == null) {
                return;
            }
            var request = new XMLHttpRequest();
            request.onreadystatechange = function (event) {
                if (request.readyState === XMLHttpRequest.DONE) {
                    var playerRatings = JSON.parse(request.response);
                    _this.createChart(playerRatings);
                }
            };
            var path = "/Ratings" + window.location.search;
            request.open("GET", path);
            request.send();
        };
        RatingChart.prototype.createChart = function (playerRatings) {
            var chart = new Highcharts.Chart({
                chart: {
                    renderTo: this.ratingChartId,
                    zoomType: "x"
                },
                credits: {
                    enabled: false
                },
                title: {
                    text: "<b>Rating chart</b>"
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
                series: [
                    {
                        name: "Match rating",
                        data: this.getSeriesData(playerRatings.matchRating)
                    },
                    {
                        name: "Kill rating",
                        data: this.getSeriesData(playerRatings.killRating)
                    }
                ]
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
        var chart = new RatingChart();
        chart.loadChartData();
    });
})(BeRated || (BeRated = {}));
//# sourceMappingURL=RatingChart.js.map