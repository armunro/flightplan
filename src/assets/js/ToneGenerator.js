export class ToneGenerator {


    playSequence(sequence) {
        let maxTime = 0.0;
        let seqArray = sequence.split("|")
        let context = new (window.AudioContext || window.webkitAudioContext)();
        let osc = context.createOscillator();
        osc.type = 'square';
        osc.connect(context.destination);
        seqArray.forEach(value => {
            let itemParts = value.split("@")
            let freq = Number.parseInt(itemParts[0].trim());
            let time = Number.parseFloat(itemParts[1].trim());

            osc.frequency.setValueAtTime(freq, time)
            if (time > maxTime) maxTime = itemParts[1];

        })
        osc.start();
        osc.stop(context.currentTime + maxTime);
    }

    playAlternating(lowFreq = 400, highFreq = 500, alternateFreq = 10, duration = 1) {
       let context = new (window.AudioContext || window.webkitAudioContext)();
        let osc = context.createOscillator();
        osc.type = 'square';
        osc.connect(context.destination);
        osc.frequency.value = highFreq
        let interval = 1 / alternateFreq
        let high = false;
        for (let i = 0; i <= duration; i += interval) {
            let freq = high ? highFreq : lowFreq;
            osc.frequency.setValueAtTime(freq, i)
            high = !high;
        }
        osc.start();
        osc.stop(context.currentTime + duration);
    }

    playSingle(freq, duration) {
        let context =  new (window.AudioContext || window.webkitAudioContext)();
        let osc = context.createOscillator();
        osc.type = 'square';
        osc.connect(context.destination);
        osc.frequency.value = freq
        osc.start();
        osc.stop(context.currentTime + duration);
    }


}