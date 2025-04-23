<script setup>
import {onMounted, ref} from "vue";

const time = ref('')


function updateClock() {
  const now = new Date()
  const hours = String(now.getHours()).padStart(2, '0')
  const minutes = String(now.getMinutes()).padStart(2, '0')
  const seconds = String(now.getSeconds()).padStart(2, '0')
  time.value = `${hours}:${minutes}:${seconds}`
}

onMounted(() => {
  updateClock()
  setInterval(updateClock, 1000)
})

</script>

<template>
  <span id="current-time">{{ time }}</span>
</template>

<style scoped>

</style>