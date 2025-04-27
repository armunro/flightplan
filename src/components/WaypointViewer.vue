<template>
  <div class="container py-3" style="max-width: 400px;">
    <form @submit.prevent>
      <div class="mb-3">
        <label class="form-label">Type</label>
        <input v-model="localWaypoint.Type" type="text" class="form-control" />
      </div>

      <div class="mb-3">
        <label class="form-label">Key</label>
        <input v-model="localWaypoint.Key" type="text" class="form-control" />
      </div>

      <div class="mb-3">
        <label class="form-label">Color</label>
        <input v-model="localWaypoint.Color" type="text" class="form-control" />
      </div>

      <div class="mb-3">
        <label class="form-label">Source</label>
        <input :value="localWaypoint.Source" type="text" class="form-control" readonly />
      </div>

      <div class="mb-3">
        <label class="form-label">Title</label>
        <input v-model="localWaypoint.Title" type="text" class="form-control" />
      </div>

      <div class="mb-3">
        <label class="form-label">Link</label>
        <input :value="localWaypoint.Link" type="text" class="form-control" readonly />
      </div>

      <div class="mb-3">
        <label class="form-label">Target Start</label>
        <input v-model="localWaypoint.TargetStart" type="datetime-local" class="form-control" />
      </div>

      <div class="mb-3">
        <label class="form-label">Spans</label>
        <div v-for="(span, index) in localWaypoint.Spans" :key="index" class="border rounded p-2 mb-2">
          <div class="mb-2">
            <label class="form-label">Span Type</label>
            <input v-model="span.Type" type="text" class="form-control" />
          </div>
          <div class="mb-2">
            <label class="form-label">Start</label>
            <input v-model="span.Start" type="datetime-local" class="form-control" />
          </div>
          <div class="mb-2">
            <label class="form-label">End</label>
            <input v-model="span.End" type="datetime-local" class="form-control" />
          </div>
          <div class="text-end">
            <button @click.prevent="removeSpan(index)" class="btn btn-danger btn-sm">Remove Span</button>
          </div>
        </div>
        <button @click.prevent="addSpan" class="btn btn-outline-primary btn-sm">Add Span</button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  waypoint: {
    type: Object,
    required: true
  }
})

// Create a local writable computed to directly edit the parent's waypoint object
const localWaypoint = computed({
  get: () => props.waypoint,
  set: (val) => {
    // no-op if needed; usually, parent controls changes
  }
})

function addSpan() {
  localWaypoint.value.Spans.push({
    Type: '',
    Start: '',
    End: ''
  })
}

function removeSpan(index) {
  localWaypoint.value.Spans.splice(index, 1)
}
</script>

<style scoped>
.container {
  background: #f8f9fa;
  border-radius: 0.5rem;
}
</style>
