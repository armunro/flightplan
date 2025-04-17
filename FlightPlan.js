import {createApp} from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js'
import FpRadar from "./components/Radar/Radar.js"
import FpRadarBackgroundLayer from "./components/Radar/Layers/RadarBackgroundLayer.js"
import FpRadarRangerLayer from "./components/Radar/Layers/RadarRangerLayer.js"
import FpRadarSymbolLayer from "./components/Radar/Layers/RadarSymbolLayer.js"
import FpCard from "./components/Card.js"
import FpCardList from "./components/CardList.js"


let app = createApp({
    data() {
        return {
            waypoints: [
                {
                    "Type": "Task",
                    "Key": "MH-123",
                    "Source": "ClickUp",
                    "Title": "Laundry - Adults",
                    "Link": "",
                    "TargetStart": "2023-09-16T17:00:00.000Z",
                    "Spans": [
                        {
                            "Type": "Work",
                            "Start": "2023-09-16T17:00:00.000Z",
                            "End": "2023-09-16T17:20:00.000Z",

                        }
                    ],
                    "Symbol": {
                        "Class": "",
                        "Unicode": "\uf219",
                        "Color": "#FFFFFF",
                    }
                },
                {
                    "Type": "Task",
                    "Key": "MH-124",
                    "Source": "ClickUp",
                    "Title": "Laundry - Kids",
                    "Link": "",
                    "TargetStart": "2023-09-16T18:00:00.000Z",
                    "Spans": [
                        {
                            "Type": "Work",
                            "Start": "2023-09-16T18:00:00.000Z",
                            "End": "2023-09-16T18:20:00.000Z",

                        }
                    ],
                    "Symbol": {
                        "Class": "",
                        "Unicode": "\uf219",
                        "Color": "#FFFFFF",
                    }

                },
                {
                    "Type": "Event",
                    "Key": "CAL: DESSAS ORTHO",
                    "Source": "O365",
                    "Title": "Dessa's Ortho",
                    "Link": "",
                    "TargetStart": "2023-09-16T19:00:00.000Z",
                    "Spans": [
                        {
                            "Type": "Attend",
                            "Start": "2023-09-16T19:00:00.000Z",
                            "End": "2023-09-16T19:00:00.000Z",
                        },
                        {
                            "Type": "Transit",
                            "Start": "2023-09-16T18:40:00.000Z",
                            "End": "2023-09-16T18:55:00.000Z",
                        }
                    ],
                    "Symbol": {
                        "Class": "",
                        "Unicode": "\uf219",
                        "Color": "#FFFFFF",
                    }

                }
            ]

        }
    },
    methods: {},
    mounted() {

    },
    watch: {}
});

app.component("fpradar", FpRadar);
app.component("fpradarbackgroundlayer", FpRadarBackgroundLayer);
app.component("fpradarrangerlayer", FpRadarRangerLayer);
app.component("fpradarsymbollayer", FpRadarSymbolLayer);
app.component("fpcard", FpCard);
app.component("fpcardlist", FpCardList);
app = app.mount("#app");
