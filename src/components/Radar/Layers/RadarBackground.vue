<template>
  <canvas id="fp-radar-background-layer"></canvas>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'

const props = defineProps({
  width: Number,
  height: Number
})

const canvas = ref(null)
const canvasElement = ref(null)

function init() {
  const ratio = 1 // window.devicePixelRatio if needed
  const el = document.getElementById('fp-radar-background-layer')
  if (!el) return

  el.width = props.width * ratio
  el.height = props.height * ratio
  el.style.width = el.width + 'px'
  el.style.height = el.height + 'px'

  canvasElement.value = el
  canvas.value = el.getContext('2d')

  if (canvas.value) {
    canvas.value.strokeStyle = '#a2a2a2'
    canvas.value.lineWidth = 2
    canvas.value.scale(ratio, ratio)
  }
}

watch(() => props.width, init)
watch(() => props.height, init)

onMounted(() => {
  init()
})
</script>

<script>
export default {
  name: 'RadarBackground'
}
</script>

<style scoped>
canvas {
  display: block;
}
</style>
