<script setup>
const props = defineProps({
  label: {
    type: String,
    required: true
  },
  value: {
    type: String,
    required: true
  }
})
</script>

<template>
  <div class="info-slot"><span class="fp-label">{{ label }}</span>
    {{ value }}
  </div>
</template>

<style scoped>

</style>