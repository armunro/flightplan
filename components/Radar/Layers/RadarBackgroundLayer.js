export default {
    props: {
        width: Number,
        height: Number
    },
    data (){
        return {
            canvas: null,
            canvasElement: null,
        }
    },
    methods: {
        init() {
            const ratio = window.devicePixelRatio;
            let canvas = document.getElementById("fp-radar-background-layer");
            canvas.width = this.width * ratio;
            canvas.height = this.height * ratio;
            canvas.style.width = canvas.width + "px";
            canvas.style.height = canvas.height + "px";
            this.canvasElement = canvas;
            this.canvas = canvas.getContext("2d");
            this.canvas.strokeStyle = "#a2a2a2";
            this.canvas.lineWidth = 2;
            this.canvas.scale(ratio, ratio);
       
        },
       
    },
    mounted() {
this.init();
    },
    template: `
        <canvas id="fp-radar-background-layer" width="{{width}}" height="{{height}}" ></canvas>
    `
}
