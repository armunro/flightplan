export default {
    props: {
        width: Number,
        height: Number,
        objects: Array
    },
    data (){
        return {
            trueWidth: 1,
            trueHeight: 1
        }
    },
    methods: {
        handleResize() {
            const radarViewport = document.getElementById("fp-radar-viewport").getBoundingClientRect();
            this.trueWidth = radarViewport.width;
            this.trueHeight = radarViewport.height;
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
            <fpradarbackgroundlayer :width="this.trueWidth" :height="trueHeight"></fpradarbackgroundlayer>
            <fpradarrangerlayer :width="trueWidth" :height="trueHeight"></fpradarrangerlayer>
            <fpradarsymbollayer :width="trueWidth" :height="trueHeight" :objects="objects"></fpradarsymbollayer>
        </div>
      
  `
}
