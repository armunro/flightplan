<script setup>
import {defineProps, onMounted} from 'vue'
import CardList from "./CardList.vue"
import Radar from "./Radar/Radar.vue"
import TabPages from "./TabPages.vue";
import Timer from "./Timer.vue";
import Events from "./Events.vue";
import PlanEditor from "./PlanEditor.vue";
import Tone from "./Tone.vue";
import Clock from "./Clock.vue";
import IconDropdown from "./IconDropdown.vue";

const { waypoints, time } = defineProps({
  waypoints: {
    type: Array,
    default: () => []
  }
})

const rightPages = [
  { title: 'Events', component: Events, },
  { title: 'Settings', component: Timer }
]
const middlePages = [
  { title: 'Radar', component: Radar, props:{ width:700, height:600,waypoints: waypoints } },
  { title: 'Plan', component: PlanEditor, props:{ waypoints: waypoints } },
  { title: 'Tone', component: Tone, props:{} },


]

onMounted(() => {
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
      <Clock></Clock>
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
      <div id="fp-radar-viewport" class="col-md-7 fp-frame overflow-hidden" style="border-right: 1px solid #134970FF">
        <TabPages :pages="middlePages"></TabPages>
      </div>
      <div class="col-md-3 fp-frame">
        <TabPages :pages="rightPages" :tab-position="'top'">  </TabPages>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Add your styles here if needed */
</style>
