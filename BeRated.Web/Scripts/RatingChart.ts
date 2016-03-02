module BeRated {
    declare var Highcharts: any;

    interface PlayerRatingSample {
        time: string;
        value: number;
    }

    interface PlayerRatings {
        name: string;
        steamId: string;
        matchRating: PlayerRatingSample[];
        killRating: PlayerRatingSample[];
    }

    interface RatingChartSeries {
        name: string;
        property: string;
    }

    interface RatingChartConfiguration {
        title: string;
        controller: string;
        enableLegend: boolean;
        series: RatingChartSeries[];
    }

    class RatingChart {
        private container: HTMLDivElement;
        private configuration: RatingChartConfiguration;

        constructor(container: HTMLDivElement) {
            this.container = container;
            this.configuration = <RatingChartConfiguration>JSON.parse(container.textContent);
            container.removeChild(container.firstChild);
        }

        loadChartData() {
            var request = new XMLHttpRequest();
            request.onreadystatechange = (event) => {
                if (request.readyState === XMLHttpRequest.DONE) {
                    var ratings = <PlayerRatings[]>JSON.parse(request.response);
                    this.createChart(ratings);
                }
            };
            var requestPath = this.configuration.controller + window.location.search;
            request.open("GET", requestPath);
            request.send();
        }

        createChart(ratings: PlayerRatings[]) {
            var series = [];
            this.configuration.series.forEach((seriesConfiguration) => {
                ratings.forEach((playerRatings) => {
                    var seriesName = seriesConfiguration.name != null ? seriesConfiguration.name : playerRatings.name;
                    var serverData = playerRatings[seriesConfiguration.property];
                    var seriesData = this.getSeriesData(serverData);
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
        }

        getSeriesData(samples: PlayerRatingSample[]) {
            return samples.map((sample) => [
                Date.parse(sample.time),
                sample.value
            ]);
        }
    }

    document.addEventListener("DOMContentLoaded", (event) => {
        var containers = <NodeListOf<HTMLDivElement>>document.querySelectorAll("div.ratingChart");
        for (var i = 0; i < containers.length; i++) {
            var container = containers[i];
            var chart = new RatingChart(container);
            chart.loadChartData();
        }
    });
}