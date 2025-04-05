export default {
    props: {
        width: Number,
        height: Number
    },
    data() {
        return {}
    },
    methods: {
        drawRangeRing(x, y, radius, dashed) {
            if (dashed)
                this.canvas.setLineDash([10, 5]);
            this.canvas.lineWidth = 1;
            this.canvas.beginPath();
            this.canvas.arc(x, y, radius, 0, 2 * Math.PI);
            this.canvas.closePath();
            this.canvas.stroke();
            this.canvas.setLineDash([]);
        },
        drawRangeCaption(text, x, y, radius, ) {
            this.canvas.fillStyle = "Cyan";
            this.canvas.font = `${16}px JetBrainsMonoRegularNerdFontComplete`;
            let coords = this.calcTextCoordinates(x, y, radius, 45);
            this.canvas.fillText(text, coords[0], coords[1]);
            coords = this.calcTextCoordinates(x, y, radius, -45);
            this.canvas.fillText(text, coords[0]-20, coords[1]);
        },
        drawRangeRings() {
            let rangeCenter = this.width / 2;
            let bottom = this.height;
            
            this.drawRangeRing(rangeCenter, bottom, 100, true);
            this.drawRangeCaption("2h", rangeCenter, bottom, 100);
            this.drawRangeRing(rangeCenter, bottom, 250, true);
            this.drawRangeCaption("4h", rangeCenter, bottom, 250);
            this.drawRangeRing(rangeCenter, bottom, 400, true);
            this.drawRangeCaption("6h", rangeCenter, bottom, 400);
            this.drawRangeRing(rangeCenter,bottom, 550);
            this.drawRangeCaption("8h", rangeCenter, bottom, 550);
            this.drawRangeRing(rangeCenter, bottom, 700);
            this.drawRangeCaption("+", rangeCenter, bottom, 700);

            
        },
        calcTextCoordinates(x_center, y_center, radius, angle) {
            let angleInRadians = (angle - 90) * (Math.PI / 180);
            let new_x = x_center + radius * Math.cos(angleInRadians);
            let new_y = y_center + radius * Math.sin(angleInRadians);
            return [new_x, new_y];
        },
        init() {

            const ratio = window.devicePixelRatio;
            let canvas = document.getElementById("fp-radar-ranger-layer");
            canvas.width = this.width * ratio;
            canvas.height = this.height * ratio;
            canvas.style.width = canvas.width + "px";
            canvas.style.height = canvas.height + "px";
            this.canvasElement = canvas;
            this.canvas = canvas.getContext("2d");
            this.canvas.strokeStyle = "#a2a2a2";
            this.canvas.lineWidth = 2
            this.canvas.scale(ratio, ratio);
        }
    },
    mounted() {
        this.init();
        this.drawRangeRings();

    },
    template: `
        <canvas id="fp-radar-ranger-layer" width="{{width}}" height="{{height}}" ></canvas>
  `
}
