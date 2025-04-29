<script setup>
import { onMounted} from 'vue'
import CardList from "./CardList.vue"
import Radar from "./Radar/Radar.vue"
import TabPages from "./TabPages.vue";
import Events from "./Events.vue";
import PlanEditor from "./PlanEditor.vue";
import Clock from "./Clock.vue";
import {planStore} from '../stores/PlanStore.js';
import WaypointViewer from "./WaypointViewer.vue";
import InfoBlock from "./InfoBlock.vue";

const store = planStore();


const rightPages = [
  //{title: 'Waypoint', component: WaypointViewer},
  {title: 'Events', component: Events,},

]
const middlePages = [
  {title: 'Radar', component: Radar, props: {width: 700, height: 600, waypoints: store.activePlan.waypoints}},
  {title: 'Plan', component: PlanEditor}
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
      <InfoBlock :value="store.activePlan.name" label="Plan"></InfoBlock>
      <InfoBlock value="" label="I2"></InfoBlock>
      <InfoBlock value="" label="I3"></InfoBlock>
      <InfoBlock value="" label="I4"></InfoBlock>
      <InfoBlock value="" label="I5"></InfoBlock>
      <InfoBlock value="" label="I6"></InfoBlock>
      <InfoBlock value="" label="I7"></InfoBlock>
      <div class="info-slot clock">
        <Clock></Clock>
      </div>
    </div>
  </nav>

  <div class="container-fluid main-container">
    <div class="row main-container">
      <div class="col-md-3 p-0 main-container">
        <div class="fp-frame">
          <CardList :waypoints="store.activePlan.waypoints"/>
        </div>
      </div>
      <div id="fp-radar-viewport" class="col-md-6 fp-frame  main-container" style="border-right: 1px solid #134970FF">
        <TabPages :pages="middlePages"></TabPages>
      </div>
      <div class="col-md-3 fp-frame main-container">
        <TabPages :pages="rightPages" :tab-position="'top'"></TabPages>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Add your styles here if needed */
</style>
