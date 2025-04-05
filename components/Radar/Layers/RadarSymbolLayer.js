export default {
    props: {
        width: Number,
        height: Number,
        objects: Array
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
        
        init() {
            const ratio = window.devicePixelRatio;
            let canvas = document.getElementById("fp-radar-symbol-layer");
            canvas.width = this.trueWidth()
            canvas.height = this.trueHeight();
            canvas.style.width = canvas.width + "px";
            canvas.style.height = canvas.height + "px";
            this.canvasElement = canvas;
            this.canvas = canvas.getContext("2d");
            this.canvas.strokeStyle = "#a2a2a2";
            this.canvas.lineWidth = 1
            this.canvas.scale(ratio, ratio);
        },
        translateY(task) {
            let date = new Date("2023-09-16T12:00:00.000Z")
            let targetDate = new Date(task.TargetStart);
            let diff = targetDate - date;
            let y = (diff / 1000 / 60 / 60) * this.height / this.hours;
            y =  (this.height - (y )) 
            console.log(y + " " + task.Key);

            return y
        },
        drawSymbol(text, x, y, color, size, symbolType) {
            this.canvas.font = ` 16px fa-sharp-regular`
            this.canvas.fillStyle = color;
            let glyph = this.typeGlyphs["Default"];
            if (this.typeGlyphs[symbolType])
                glyph = this.typeGlyphs[symbolType];
            this.canvas.fillText(glyph, x , y, 45);
            this.canvas.font = `${size}px JetBrainsMonoRegularNerdFontComplete`;
            this.canvas.fillText(text, x + 20, y);
        },
        drawPlannedSymbols(objects, drawPlanLine) {
            let lastX = 0;
            let lastY = 0;
            objects.forEach((task, index) => {
                let drift = 0;
                if(index >0)
                    drift = this.randomDrift(-150, 150)
                let x = ((this.width) / 2) + drift;
                let y = this.translateY(task);
                this.drawSymbol(task.Key, x, y, "#CC6CE7", 16, task.Source);

                if (lastX > 0 && lastY > 0 && drawPlanLine)
                    this.drawWaypointLines(x +10, y - 8, lastX +10, lastY - 8);
                lastX = x;
                lastY = y;
            });
        },
        drawUnplannedSymbols(objects) {
            let lastX = 0;
            let lastY = 0;
            let maxDrift = this.width / 2;
            objects.forEach((task, index) => {
                let drift = this.randomDrift(-1 * maxDrift, maxDrift)
                let x = ((this.width) / 2) + drift;
                let y = this.translateY(task) + drift;
                this.drawSymbol(task.Key, x, y, "White", 16, task.Source);
                lastX = x;
                lastY = y;
            });
        },
        drawWaypointLines(xTo, yTo, xFrom, yFrom) {
            this.canvas.strokeStyle = "#CC6CE7";
            this.canvas.beginPath();
            this.canvas.moveTo(xFrom, yFrom);
            this.canvas.lineTo(xTo, yTo);
            this.canvas.stroke();
        }
        ,
        randomDrift(min, max) {
            return Math.random() * (max - min) + min;
        }
    },
    mounted() {
        const fa_sharp_regular = new FontFace('fa-sharp-regular',
            'url(./assets/fonts/fa-pro-sharp-regular/webfonts/fa-sharp-regular-400.woff2)');
        document.fonts.add(fa_sharp_regular);
        fa_sharp_regular.load().then(() => {
            this.init()
            this.drawPlannedSymbols(this.objects, true);
            
        }).catch(console.error);
    },
    template: `
        <canvas id="fp-radar-symbol-layer" width="{{width}}" height="{{height}}" ></canvas>
  `
}
