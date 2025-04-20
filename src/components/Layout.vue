<script setup>
import { defineProps } from 'vue'
import CardList from "./CardList.vue"
import Radar from "./Radar/Radar.vue"

const props = defineProps({
  waypoints: {
    type: Array,
    default: () => []
  },
  time: {
    type: String,
    default: () => ""
  }
})
</script>

<template>
  <nav class="navbar navbar-expand-lg navbar-light">
    <div class="d-flex flex-fill flex-wrap justify-content-start w-100">
      <div class="info-slot p-3">
        <a class="navbar-brand" href="#">FlightPlan</a>
      </div>
      <div class="info-slot"><span class="fp-label">I1</span></div>
      <div class="info-slot"><span class="fp-label">I2</span></div>
      <div class="info-slot"><span class="fp-label">I3</span></div>
      <div class="info-slot"><span class="fp-label">I4</span></div>
      <div class="info-slot"><span class="fp-label">I5</span></div>
      <div class="info-slot"><span class="fp-label">I6</span></div>
      <div class="info-slot"><span class="fp-label">I7</span></div>
      <div class="info-slot clock">
        <span id="current-time">{{ time }}</span>
      </div>
    </div>
  </nav>

  <div class="container-fluid main-container">
    <div class="row main-container">
      <div class="col-md-2 p-0">
        <div class="fp-frame">
          <CardList :waypoints="waypoints" />
        </div>
      </div>
      <div id="fp-radar-viewport" class="col-md-7 p-0 overflow-hidden" style="border-right: 1px solid #134970FF">
        <Radar :width="700" :height="600" :waypoints="waypoints" />
      </div>
      <div class="col-md-3 p-0">
        <!-- Additional panel content -->
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Add your styles here if needed */
</style>
