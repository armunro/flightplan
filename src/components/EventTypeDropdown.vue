<template>
  <div class="dropdown">
    <button
        class="btn btn-outline-primary dropdown-toggle w-100 text-start d-flex align-items-center gap-2"
        type="button"
        data-bs-toggle="dropdown"
        aria-expanded="false"
    >
      <i v-if="selectedEvent" :class="['fa', selectedEvent.icon, 'text-primary']"></i>
      <span>{{ selectedEvent?.name || 'Type' }}</span>
    </button>
    <ul class="dropdown-menu p-2" style="max-height: 300px; width:400px !important; overflow-y: auto;">
      <li>
        <input
            v-model="filter"
            type="text"
            class="form-control mb-2"
            placeholder="Search event types..."
        />
      </li>
      <li
          v-for="event in filteredEventTypes"
          :key="event.name"
          class="dropdown-item d-flex align-items-start gap-2"
          @click="selectType(event)"
          style="cursor: pointer;"
      >
        <i :class="['fa', event.icon, 'text-primary']"></i>
        <div>
          <div class="fw-bold">{{ event.name }}</div>
          <small class="text-muted">{{ event.description }}</small>
        </div>
      </li>
      <li v-if="filteredEventTypes.length === 0" class="dropdown-item text-muted">
        No results found.
      </li>
    </ul>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'
import {typesStore} from "../stores/TypesStore.js";
const typeStore = typesStore();

const props = defineProps({
  modelValue: String,
})

const emit = defineEmits(['update:modelValue'])

const filter = ref('')

const eventTypes = typeStore.waypointTypes

const selectedEvent = computed(() =>
    eventTypes.find(event => event.name === props.modelValue) || null
)

const filteredEventTypes = computed(() =>
    eventTypes.filter(event =>
        event.name.toLowerCase().includes(filter.value.toLowerCase()) ||
        event.description.toLowerCase().includes(filter.value.toLowerCase())
    )
)

function selectType(event) {
  emit('update:modelValue', event.name)
}
</script>
