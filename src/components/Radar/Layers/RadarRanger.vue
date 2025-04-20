<template>
  <canvas id="fp-radar-ranger-layer"></canvas>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'

const props = defineProps({
  width: Number,
  height: Number
})

const canvas = ref(null)
const canvasElement = ref(null)

function calcTextCoordinates(x_center, y_center, radius, angle) {
  const angleInRadians = (angle - 90) * (Math.PI / 180)
  const new_x = x_center + radius * Math.cos(angleInRadians)
  const new_y = y_center + radius * Math.sin(angleInRadians)
  return [new_x, new_y]
}

function drawRangeRing(x, y, radius, dashed = false) {
  if (!canvas.value) return
  if (dashed) canvas.value.setLineDash([10, 5])
  canvas.value.lineWidth = 1
  canvas.value.beginPath()
  canvas.value.arc(x, y, radius, 0, 2 * Math.PI)
  canvas.value.closePath()
  canvas.value.stroke()
  canvas.value.setLineDash([])
}

function drawRangeCaption(text, x, y, radius) {
  if (!canvas.value) return
  canvas.value.fillStyle = 'Cyan'
  canvas.value.font = '13px JetBrainsMonoRegularNerdFontComplete'

  let coords = calcTextCoordinates(x, y, radius, 45)
  canvas.value.fillText(text, coords[0], coords[1])

  coords = calcTextCoordinates(x, y, radius, -45)
  canvas.value.fillText(text, coords[0] - 20, coords[1])
}

function drawRangeRings() {
  const rangeCenter = props.width / 2
  const bottom = props.height

  drawRangeRing(rangeCenter, bottom, 100, true)
  drawRangeCaption('2h', rangeCenter, bottom, 100)
  drawRangeRing(rangeCenter, bottom, 250, true)
  drawRangeCaption('4h', rangeCenter, bottom, 250)
  drawRangeRing(rangeCenter, bottom, 400, true)
  drawRangeCaption('6h', rangeCenter, bottom, 400)
  drawRangeRing(rangeCenter, bottom, 550)
  drawRangeCaption('8h', rangeCenter, bottom, 550)
  drawRangeRing(rangeCenter, bottom, 700)
  drawRangeCaption('+', rangeCenter, bottom, 700)
}

function init() {
  const ratio = 1
  const el = document.getElementById('fp-radar-ranger-layer')
  if (!el) return

  el.width = props.width * ratio
  el.height = props.height * ratio
  el.style.width = el.width + 'px'
  el.style.height = el.height + 'px'

  canvasElement.value = el
  canvas.value = el.getContext('2d')
  canvas.value.strokeStyle = '#a2a2a2'
  canvas.value.lineWidth = 2
  canvas.value.scale(ratio, ratio)
}

function handlePropChange() {
  init()
  drawRangeRings()
}

// Watch for reactive prop changes
watch(() => props.width, handlePropChange)
watch(() => props.height, handlePropChange)

onMounted(() => {
  init()
  drawRangeRings()
})
</script>

<script>
export default {
  name: 'RadarRanger'
}
</script>

<style scoped>
canvas {
  display: block;
}
</style>
