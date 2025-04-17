export default {
    props: {
        width: Number,
        height: Number,
        waypoints: Array
    },
    watch: {
        width() {
            this.handlePropChange();
        },
        height() {
            this.handlePropChange();
        }
    },
    data() {
        return {
            symbolYOffset: -30,
            hours: 8.6,
            canvas: null,
            canvasElement: null,
            typeGlyphs: {
                "Jira": "\uf219", //diamond
                "Case": "\uf7d4", //weird star
                "Default": "\uf60b" //question pin
            }
        }
    },
    methods: {
        trueHeight() {return this.height * window.devicePixelRatio},
        trueWidth() {return this.width * window.devicePixelRatio},
        handlePropChange() {
            this.init();
        },
        init() {

        },
        translateY(task) {
            let date = new Date("2023-09-16T12:00:00.000Z")
            let targetDate = new Date(task.TargetStart);
            let diff = targetDate - date;
            let y = (diff / 1000 / 60 / 60) * this.height / this.hours;
            y =  (this.height - (y )) 

            return y
        },
        translateSpanY(span) {
            let date = new Date("2023-09-16T12:00:00.000Z")
            let targetDate = new Date(span.Start);
            let diff = targetDate - date;
            let y = (diff / 1000 / 60 / 60) * this.height / this.hours;
            y =  (this.height - (y ))

            return y
        },
        translateX(key) {
            let drift = 0;
          
                drift = this.hashDrift(key, -250, 250);
                 return (this.width / 2) + drift;
        },
        hashDrift(key, min, max) {
            let hash = 0;
            for (let i = 0; i < key.length; i++) {
                hash = (hash << 5) - hash + key.charCodeAt(i);
                hash |= 0; // Convert to 32bit integer
            }
            // Normalize hash to 0..1 and scale to min..max
            const range = max - min;
            const normalized = (hash >>> 0) / 0xFFFFFFFF;
            return min + normalized * range;
        },

       
    },
    mounted() {
            this.init()
    },
    template: `
        <div id="fp-radar-symbol-layer" :style="{'width': width + 'px', 'height': height + 'px'}">
        <template v-for="(wp, index) in waypoints" >
            <div class="fp-radar-symbol" :style="{'top': this.translateY(wp) + 'px', 'left': this.translateX(wp.Key) +'px'}">
                 <i class="fa-regular fa-square"></i> <span class="fp-label">{{ wp.Key}}</span>
            </div>
            <div v-for="(span, index) in wp.Spans" class="fp-radar-spanline" :style="{'top': this.translateSpanY(span) + 'px', 'left': this.translateX(wp.Key) +'px'}">
                
            </div>
            </template>
        </div>
  `
}
