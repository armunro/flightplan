<script setup>
import Layout from "./components/Layout.vue";
import {planStore} from './stores/PlanStore.js';
const store = planStore();


store.load()
store.activatePlan(store.plans[0]);


</script>

<template>
  <Layout :waypoints="store.activePlan.waypoints"></Layout>
</template>

<style scoped>

</style>
