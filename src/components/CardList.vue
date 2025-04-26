<template>
  <div class="fp-card-list">
    <Card v-for="(wp, index) in waypoints" :key="index" :data="wp" />
  </div>
</template>

<script>
import Card from "./Card.vue";

export default {
  name: 'CardList',
  components: {Card},
  props: {
    waypoints: {
      name: Array,
      required: true
    }
  }
}
</script>

<style scoped>
/* Add component-specific styles here if needed */
</style>
