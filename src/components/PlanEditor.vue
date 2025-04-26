<template>
  <div class="container py-4">

    <div class="mb-3">
      <label class="form-label">Plan</label>
      <input v-model="store.activePlan.name" class="form-control" placeholder="Enter plan name"/>
    </div>

    <div class="mb-3">
      <button class="btn btn-success" @click="addWaypoint">Add Waypoint</button>
      <button class="btn btn-warning" @click="importOutlook">Import From Outlook</button>
    </div>

    <div v-for="(wp, index) in store.activePlan.waypoints" :key="index" class="card mb-3">
      <div class="card-header position-relative">
        <i :class="['fa-regular', getIconClass(wp.Type)]" :style="{color: wp.Color}"></i>&nbsp;Waypoint {{ index + 1 }}
        <button type="button" class="btn-close position-absolute top-0 end-0 m-2" @click="removeWaypoint(index)"
                aria-label="Close"></button>
      </div>
      <div class="card-body">


        <div class="row g-2">
          <div class="col-md-2">
            <label class="form-label">Type</label>
            <EventTypeDropdown v-model="wp.Type"></EventTypeDropdown>
          </div>
          <div class="col-md-1">
            <label class="form-label"> Color</label>
            <input v-model="wp.Color" type="color" class="form-control form-control-color w-100"/>
          </div>
          <div class="col-md-3">
            <label class="form-label">Key</label>
            <input v-model="wp.Key" class="form-control"/>
          </div>
          <div class="col-md-3">
            <label class="form-label">Source</label>
            <input v-model="wp.Source" class="form-control"/>
          </div>
          <div class="col-md-3">
            <label class="form-label">Title</label>
            <input v-model="wp.Title" class="form-control"/>
          </div>
        </div>


        <div class="mt-3">
          <h6>Spans</h6>
          <div v-for="(span, sIndex) in wp.Spans" :key="sIndex" class="row g-2 mb-2">
            <div class="col-md-3">

              <input v-model="span.Type" class="form-control" placeholder="Type"/>
            </div>
            <div class="col-md-4">
              <input v-model="span.Start" type="datetime-local" class="form-control" placeholder="Start"/>
            </div>
            <div class="col-md-4">
              <input v-model="span.End" type="datetime-local" class="form-control" placeholder="End"/>
            </div>
            <div class="col-md-1">
              <button class="btn btn-outline-danger" @click="removeSpan(index, sIndex)">X</button>
            </div>
          </div>
          <button class="btn btn-outline-primary btn-sm" @click="addSpan(index)">Add Span</button>
        </div>
      </div>
    </div>

    <pre class="mt-4">{{ JSON.stringify(store.activePlan, null, 2) }}</pre>
  </div>
</template>

<script setup>
import {reactive, ref} from 'vue';
import IconDropdown from "./IconDropdown.vue";
import {planStore} from '../stores/PlanStore.js';
import {typesStore} from "../stores/TypesStore.js";
import EventTypeDropdown from "./EventTypeDropdown.vue";

const store = planStore();
const typStore = typesStore();

const getTodayName = () => {
  const today = new Date();
  return today.toISOString().split('T')[0] + ' Daily';
};


function addWaypoint() {
  store.activePlan.waypoints.push({
    Type: 'Task',
    Key: '',
    Source: '',
    Title: '',
    Link: '',
    TargetStart: '',
    Spans: [],
    Symbol: {Class: '', Unicode: '', Color: '#000000'}
  });
}

function removeWaypoint(index) {
  store.activePlan.waypoints.splice(index, 1);
}

function addSpan(wpIndex) {
  store.activePlan.waypoints[wpIndex].Spans.push({
    Type: '',
    Start: '',
    End: ''
  });
}

function removeSpan(wpIndex, sIndex) {
  store.activePlan.waypoints[wpIndex].Spans.splice(sIndex, 1);
}

function getIconClass(waypointType){
  return typStore.getIconClass(waypointType)
}
</script>

<style scoped>
.card-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
