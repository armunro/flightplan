<script setup>
import { ref } from 'vue'
import {ToneGenerator} from "../assets/js/ToneGenerator.js";

const toneGenerator = new ToneGenerator();
const singleFreq = ref(50);
const singleDuration = ref(1);
const duration = ref(1);
const freqHi = ref(100);
const freqLow = ref(50);
const freqAlt = ref(100);
const sequence = ref("");


</script>
<template>
  <div class="container text-center" style="background-color: #1a1e21">
    <h2>Single Tone</h2>
    <div class="mb-2 text-start">
      <label class="form-label">Freq (Hz)</label>
      <input v-model="singleFreq" type="text" class="form-control" placeholder="800" />
    </div>
    <div class="mb-2 text-start">
      <label class="form-label">Duration (s)</label>
      <input v-model="singleDuration" type="text" class="form-control" placeholder="1" />
    </div>
    <div class="mb-3">
      <button class="btn btn-success" @click="playSingle">Play</button>
    </div>

    <h2>Alternating (Siren)</h2>
    <div class="mb-2 text-start">
      <label class="form-label">High (Hz)</label>
      <input v-model="freqHi" type="text" class="form-control" />
    </div>
    <div class="mb-2 text-start">
      <label class="form-label">Low (Hz)</label>
      <input v-model="freqLow" type="text" class="form-control" />
    </div>
    <div class="mb-2 text-start">
      <label class="form-label">Alternate (Hz)</label>
      <input v-model="freqAlt" type="text" class="form-control" />
    </div>
    <div class="mb-2 text-start">
      <label class="form-label">Duration (s)</label>
      <input v-model="duration" type="text" class="form-control" />
    </div>
    <div class="mb-3">
      <button class="btn btn-warning" @click="playAlternating">Play</button>
    </div>

    <h2>Text Sequencer</h2>
    <div class="mb-2 text-start">
      <label class="form-label">Sequence {freq}@{timeMs} | ...</label>
      <input v-model="sequence" type="text" class="form-control" />
    </div>
    <div class="mb-3">
      <button class="btn btn-danger" @click="playSequence">Play</button>
    </div>

    <h2>Premade</h2>
    <div class="mb-3 d-flex flex-column gap-2">
      <div>
        <strong class="text-light me-2">RWR</strong>
        <button class="btn btn-success me-1" @click="toneGenerator.playSingle(460, 0.25)">Contact</button>
        <button class="btn btn-danger me-1" @click="toneGenerator.playSequence('575@0 | 550@.1 | 525@.2 | 500@.3 | 0@.4')">Special Contact</button>
        <button class="btn btn-warning me-1" @click="toneGenerator.playAlternating(570, 380, 3, 2)">Lock</button>
        <button class="btn btn-warning me-1" @click="toneGenerator.playAlternating(570, 380, 12, 2)">Launch</button>
        <button class="btn btn-danger" @click="toneGenerator.playSequence('2000@0 | 2700@.30 | 2000@.50 | 2700@.80 | 0@1')">MASTER</button>
      </div>
    </div>

    <div class="mb-3 d-flex flex-column gap-2">
      <div>
        <strong class="text-light me-2">Prompt</strong>
        <button class="btn btn-danger me-1" @click="ToneGenerator.playSequence('400@0 | 300@.1 | 500@.2 | 0@.3')">Success</button>
        <button class="btn btn-danger me-1" @click="toneGenerator.playSequence('500@0 | 400@.1 | 0@.2')">Confirmed</button>
        <button class="btn btn-success" @click="toneGenerator.playSingle(400, 0.1)">Entered</button>
      </div>
    </div>
  </div>
</template>
