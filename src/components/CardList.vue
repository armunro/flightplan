<template>
  <div class="fp-card-list">
    <Card v-for="(wp, index) in waypoints" :key="index" :data="wp" />
  </div>
</template>

<script setup>
import { defineProps } from 'vue'
import Card from './Card.vue'

const props = defineProps({
  waypoints: {
    type: Array,
    required: true
  }
})
</script>

<style scoped>
/* Add component-specific styles here if needed */
</style>
