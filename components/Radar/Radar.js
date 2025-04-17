export default {
    props: {
        width: Number,
        height: Number,
        waypoints: Array
    },
    data (){
        return {
            trueWidth: 800,
            trueHeight: 600
        }
    },
    methods: {
        handleResize() {
            const radarViewport = document.getElementById("fp-radar-viewport").getBoundingClientRect();
            this.trueWidth = radarViewport.width;
            this.trueHeight = window.innerHeight - 57;
        }
    },
    mounted() {
        window.addEventListener('resize', this.handleResize);
        this.handleResize();
    },
    beforeDestroy() {
        window.removeEventListener('resize', this.handleResize);
    },
    template: `
    
        <div class="fp-radar-container" :style="{ width: trueWidth + 'px', height: trueHeight + 'px', float: 'left' }" >
            <fpradarbackgroundlayer :width="trueWidth" :height="trueHeight"></fpradarbackgroundlayer>
            <fpradarrangerlayer :width="trueWidth" :height="trueHeight"></fpradarrangerlayer>
            <fpradarsymbollayer :width="trueWidth" :height="trueHeight" :waypoints="waypoints"></fpradarsymbollayer>
        </div>
      
  `
}
