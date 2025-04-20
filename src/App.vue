<script setup>
import {ref, reactive, onMounted} from 'vue'
import Layout from "./components/Layout.vue";

// Time display
const time = ref('')

// Waypoints data
const waypoints = ref([
  {
    Type: 'Task',
    Key: 'MH-123',
    Source: 'ClickUp',
    Title: 'Laundry - Adults',
    Link: '',
    TargetStart: '2023-09-16T17:00:00.000Z',
    Spans: [
      {Type: 'Work', Start: '2023-09-16T17:00:00.000Z', End: '2023-09-16T17:20:00.000Z'}
    ],
    Symbol: {Class: '', Unicode: '\uf219', Color: '#FFFFFF'}
  },
  {
    Type: 'Task',
    Key: 'MH-124',
    Source: 'ClickUp',
    Title: 'Laundry - Kids',
    Link: '',
    TargetStart: '2023-09-16T18:00:00.000Z',
    Spans: [
      {Type: 'Work', Start: '2023-09-16T18:00:00.000Z', End: '2023-09-16T18:20:00.000Z'}
    ],
    Symbol: {Class: '', Unicode: '\uf219', Color: '#FFFFFF'}
  },
  {
    Type: 'Event',
    Key: 'CAL: DESSAS ORTHO',
    Source: 'O365',
    Title: "Dessa's Ortho",
    Link: '',
    TargetStart: '2023-09-16T19:00:00.000Z',
    Spans: [
      {Type: 'Attend', Start: '2023-09-16T19:00:00.000Z', End: '2023-09-16T19:00:00.000Z'},
      {Type: 'Transit', Start: '2023-09-16T18:40:00.000Z', End: '2023-09-16T18:55:00.000Z'}
    ],
    Symbol: {Class: '', Unicode: '\uf219', Color: '#FFFFFF'}
  }
])

// MSAL API config
const msalInstance = ref(null)
const account = ref({})
const loginRequest = {
  scopes: ['User.Read', 'Calendars.Read']
}

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
  <Layout :waypoints="waypoints" :time="time"></Layout>
</template>

<style scoped>

</style>
