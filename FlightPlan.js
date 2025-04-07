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
            objects: [
                {
                    "Key": "J-1371",
                    "Source": "Jira",
                    "Title": "",
                    "Link": "",
                    "TargetStart": "2023-09-16T12:00:00.000Z"
                },
                {
                    "Key": "J-1371",
                    "Source": "Jira",
                    "Title": "",
                    "Link": "",
                    "TargetStart": "2023-09-16T13:00:00.000Z"

                },
                {
                    "Key": "J-1299",
                    "Source": "Jira",
                    "Title": "",
                    "Link": "",
                    "TargetStart": "2023-09-16T14:00:00.000Z"
                },
                {
                    "Key": "J-1262",
                    "Source": "Jira",
                    "Title": "",
                    "Link": "",
                    "TargetStart": "2023-09-16T15:00:00.000Z"
                    
                },
                {
                    "Key": "15698504",
                    "Source": "Case",
                    "Title": "",
                    "Link": "",
                    "TargetStart": "2023-09-16T16:00:00.000Z"
                    
                },
                {
                    "Key": "14114681",
                    "Source": "Case",
                    "Title": "",
                    "Link": "",
                    "TargetStart": "2023-09-16T17:00:00.000Z"
                    
                },
                {
                    "Key": "13388161",
                    "Source": "Other",
                    "Title": "",
                    "Link": "",
                    "TargetStart": "2023-09-16T18:00:00.000Z"
                    
                },
                {
                    "Key": "13388161",
                    "Source": "Other",
                    "Title": "",
                    "Link": "",
                    "TargetStart": "2023-09-16T19:00:00.000Z"

                },
                {
                    "Key": "13388161",
                    "Source": "Other",
                    "Title": "",
                    "Link": "",
                    "TargetStart": "2023-09-16T20:00:00.000Z"

                }
            ]
            
        }
    },
    methods: {
        
    },
    mounted() {
        
    },
    watch: {}
});

app.component("fpradar",FpRadar);
app.component("fpradarbackgroundlayer",FpRadarBackgroundLayer);
app.component("fpradarrangerlayer",FpRadarRangerLayer);
app.component("fpradarsymbollayer",FpRadarSymbolLayer);
app.component("fpcard",FpCard);
app.component("fpcardlist",FpCardList);
app = app.mount("#app");
