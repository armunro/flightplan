﻿export default {
    props: {
        width: Number,
        height: Number,
        objects: Array
    },
    data (){
        return {
            
        }
    },
    methods: {
        
    },
    mounted() {
        
    },
    template: `
    <div class="fp-radar-container" :style="{width: width, height: height}" >
        <fpradarbackgroundlayer :width="width" :height="height"></fpradarbackgroundlayer>
        <fpradarrangerlayer :width="width" :height="height"></fpradarrangerlayer>
        <fpradarsymbollayer :width="width" :height="height" :objects="objects"></fpradarsymbollayer>
    </div>
  `
}
