module BeRated {
    declare var Highcharts: any;

    interface PlayerRatingSample {
        time: string;
        value: number;
    }

    interface PlayerRatings {
        matchRating: PlayerRatingSample[];
        killRating: PlayerRatingSample[];
    }

    class RatingChart {
        loadChartData() {
            var request = new XMLHttpRequest();
            request.onreadystatechange = (event) => {
                if (request.readyState === XMLHttpRequest.DONE) {
                    var playerRatings = <PlayerRatings>JSON.parse(request.response);
                    this.createChart(playerRatings);
                }
            };
            var path = "/Ratings" + window.location.search;
            request.open("GET", path);
            request.send();
        }

        createChart(playerRatings: PlayerRatings) {
            var chart = new Highcharts.Chart({
                chart: {
                    renderTo: "ratingChart",
                    zoomType: "x"
                },
                credits: {
                    enabled: false
                },
                title: {
                    text: "<b>TrueSkill rating chart</b>"
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
                    },
                    min: 0
                },
                tooltip: {
                    headerFormat: "<b>{series.name}</b><br>",
                    pointFormat: "{point.x:%Y-%m-%d %H:%M:%S}: {point.y:,.1f}"
                },
                series: [
                    {
                        name: "M-Rating",
                        data: this.getSeriesData(playerRatings.matchRating)
                    },
                    {
                        name: "K-Rating",
                        data: this.getSeriesData(playerRatings.killRating)
                    }
                ]
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
        var chart = new RatingChart();
        chart.loadChartData();
    });
}