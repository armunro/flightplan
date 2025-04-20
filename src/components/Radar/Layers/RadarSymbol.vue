<template>
  <div id="fp-radar-symbol-layer" :style="{ width: width + 'px', height: height + 'px' }">
    <template v-for="(wp, index) in waypoints" :key="wp.Key">
      <div
          class="fp-radar-symbol"
          :style="{ top: translateY(wp) + 'px', left: translateX(wp.Key) + 'px' }"
      >
        <i class="fa-regular fa-square"></i>
        <span class="fp-label">{{ wp.Key }}</span>
      </div>
      <div
          v-for="(span, spanIndex) in wp.Spans"
          :key="spanIndex"
          class="fp-radar-spanline"
          :style="{ top: translateSpanY(span) + 'px', left: translateX(wp.Key) + 'px' }"
      ></div>
    </template>
  </div>
</template>

<script setup>
import { watch, onMounted } from 'vue'

const props = defineProps({
  width: Number,
  height: Number,
  waypoints: Array
})

const symbolYOffset = -30
const hours = 8.6

const typeGlyphs = {
  Jira: '\uf219',
  Case: '\uf7d4',
  Default: '\uf60b'
}

function trueHeight() {
  return props.height * window.devicePixelRatio
}

function trueWidth() {
  return props.width * window.devicePixelRatio
}

function init() {
  // Placeholder for canvas init if needed later
}

function translateY(task) {
  const date = new Date('2023-09-16T12:00:00.000Z')
  const targetDate = new Date(task.TargetStart)
  const diff = targetDate - date
  let y = (diff / 1000 / 60 / 60) * props.height / hours
  return props.height - y
}

function translateSpanY(span) {
  const date = new Date('2023-09-16T12:00:00.000Z')
  const targetDate = new Date(span.Start)
  const diff = targetDate - date
  let y = (diff / 1000 / 60 / 60) * props.height / hours
  return props.height - y
}

function translateX(key) {
  const drift = hashDrift(key, -250, 250)
  return props.width / 2 + drift
}

function hashDrift(key, min, max) {
  let hash = 0
  for (let i = 0; i < key.length; i++) {
    hash = (hash << 5) - hash + key.charCodeAt(i)
    hash |= 0
  }
  const range = max - min
  const normalized = (hash >>> 0) / 0xffffffff
  return min + normalized * range
}

function handlePropChange() {
  init()
}

watch(() => props.width, handlePropChange)
watch(() => props.height, handlePropChange)

onMounted(() => {
  init()
})
</script>

<script>
export default {
  name: 'RadarSymbol'
}
</script>

<style scoped>
.fp-radar-symbol {
  position: absolute;
}

.fp-radar-spanline {
  position: absolute;
  width: 2px;
  height: 10px;
  background-color: #ccc;
}
</style>
