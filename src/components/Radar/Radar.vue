<template>
  <div
      id="fp-radar-viewport"
      class="fp-radar-container"
      :style="{ width: trueWidth + 'px', height: trueHeight + 'px', float: 'left' }"
  >
    <RadarBackground :width="trueWidth" :height="trueHeight" />
    <RadarRanger :width="trueWidth" :height="trueHeight" />
    <RadarSymbol :width="trueWidth" :height="trueHeight" :waypoints="waypoints" />
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import RadarBackground from "./Layers/RadarBackground.vue";
import RadarRanger from "./Layers/RadarRanger.vue";
import RadarSymbol from "./Layers/RadarSymbol.vue";


const props = defineProps({
  width: Number,
  height: Number,
  waypoints: Array
})

const trueWidth = ref(800)
const trueHeight = ref(600)

function handleResize() {
  const radarViewport = document.getElementById('fp-radar-viewport')?.getBoundingClientRect()
  if (radarViewport) {
    trueWidth.value = radarViewport.width
    trueHeight.value = window.innerHeight - 57
  }
}

onMounted(() => {
  window.addEventListener('resize', handleResize)
  handleResize()
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<script>
export default {
  name: 'Radar'
}
</script>

<style scoped>
/* Add styles if needed */
</style>
