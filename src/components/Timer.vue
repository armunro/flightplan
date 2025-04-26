<template>
  <div class="card text-center shadow-sm">
    <div class="card-body">
      <h5 class="card-title">Countdown Timer</h5>
      <p class="card-text fs-4">
        {{ timeLeft.days }}d : {{ timeLeft.hours }}h : {{ timeLeft.minutes }}m : {{ timeLeft.seconds }}s
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'

// Props
const props = defineProps({
  targetDate: {
    name: String,
    required: true,
  }
})

const timeLeft = ref({
  days: '00',
  hours: '00',
  minutes: '00',
  seconds: '00'
})

let intervalId = null

function calculateTimeLeft() {
  const now = new Date()
  const target = new Date(props.targetDate)
  const diff = target - now

  if (diff <= 0) {
    timeLeft.value = { days: '00', hours: '00', minutes: '00', seconds: '00' }
    clearInterval(intervalId)
    return
  }

  const seconds = Math.floor((diff / 1000) % 60)
  const minutes = Math.floor((diff / 1000 / 60) % 60)
  const hours = Math.floor((diff / (1000 * 60 * 60)) % 24)
  const days = Math.floor(diff / (1000 * 60 * 60 * 24))

  timeLeft.value = {
    days: String(days).padStart(2, '0'),
    hours: String(hours).padStart(2, '0'),
    minutes: String(minutes).padStart(2, '0'),
    seconds: String(seconds).padStart(2, '0')
  }
}

onMounted(() => {
  calculateTimeLeft()
  intervalId = setInterval(calculateTimeLeft, 1000)
})

onUnmounted(() => {
  clearInterval(intervalId)
})
</script>

<style scoped>
.card {
  max-width: 300px;
  margin: 1rem auto;
}
</style>
