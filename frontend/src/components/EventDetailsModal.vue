<template>
  <div v-if="isOpen" class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-container theme-bg-darker theme-border theme-shadow">
      <div class="modal-header d-flex align-items-center justify-content-between p-3 border-bottom theme-border">
        <h5 class="mb-0 theme-text">Event Details</h5>
        <button class="btn-close btn-close-white" @click="$emit('close')"></button>
      </div>
      
      <div class="modal-body p-3">
        <div class="event-info mb-4">
          <h4 class="theme-text mb-2">{{ event.title }}</h4>
          <div class="d-flex align-items-center gap-2 mb-2 theme-text-muted">
            <i class="bi bi-calendar3"></i>
            <span>{{ formatDateTime(event.start, event.end, event.allDay) }}</span>
          </div>
          <div v-if="event.extendedProps?.location" class="d-flex align-items-center gap-2 mb-2 theme-text-muted">
            <i class="bi bi-geo-alt"></i>
            <span>{{ event.extendedProps.location }}</span>
          </div>
          <div v-if="calendarName" class="d-flex align-items-center gap-2 mb-2 theme-text-muted">
            <i class="bi bi-bookmark-fill" :style="{ color: event.backgroundColor }"></i>
            <span>{{ calendarName }}</span>
          </div>
        </div>

        <div class="d-flex justify-content-between gap-2 mt-4">
          <div>
            <button v-if="event.url" class="btn btn-primary me-2" @click="openLink">
              <i class="bi bi-box-arrow-up-right me-1"></i> Open in Outlook
            </button>
          </div>
          <div class="d-flex gap-2">
            <button type="button" class="btn btn-secondary" @click="$emit('close')">Close</button>
            <button v-if="canEdit" type="button" class="btn btn-primary" @click="$emit('edit', event)">Edit</button>
            <button v-if="canDelete" type="button" class="btn btn-danger" @click="handleDelete" :disabled="isDeleting">
              <span v-if="isDeleting" class="spinner-border spinner-border-sm me-1"></span>
              Delete
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { showToast } from './Toast.vue';

const props = defineProps({
  isOpen: Boolean,
  event: {
    type: Object,
    default: () => ({})
  },
  calendars: {
    type: Array,
    default: () => []
  },
  canEdit: {
    type: Boolean,
    default: true
  },
  canDelete: {
    type: Boolean,
    default: true
  }
});

const emit = defineEmits(['close', 'edit', 'delete']);

const isDeleting = ref(false);

const calendarName = computed(() => {
  const calId = props.event.extendedProps?.calendarId;
  if (!calId) return '';
  const calendar = props.calendars.find(c => c.id === calId);
  return calendar ? calendar.displayName : '';
});

const formatDateTime = (start, end, allDay) => {
  if (!start) return '';
  const startDate = new Date(start);
  const endDate = end ? new Date(end) : null;

  const dateOptions = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
  const timeOptions = { hour: 'numeric', minute: '2-digit' };

  if (allDay) {
    // For all-day events, the end date is exclusive, so we subtract one day for display if it's more than one day
    const displayEndDate = new Date(startDate);
    if (endDate && endDate.getTime() > startDate.getTime() + 24 * 60 * 60 * 1000) {
        const lastDay = new Date(endDate);
        lastDay.setDate(lastDay.getDate() - 1);
        return `${startDate.toLocaleDateString(undefined, dateOptions)} - ${lastDay.toLocaleDateString(undefined, dateOptions)}`;
    }
    return startDate.toLocaleDateString(undefined, dateOptions);
  }

  const startStr = startDate.toLocaleDateString(undefined, dateOptions) + ' ' + startDate.toLocaleTimeString(undefined, timeOptions);
  if (endDate) {
    const isSameDay = startDate.toDateString() === endDate.toDateString();
    const endStr = isSameDay 
      ? endDate.toLocaleTimeString(undefined, timeOptions)
      : endDate.toLocaleDateString(undefined, dateOptions) + ' ' + endDate.toLocaleTimeString(undefined, timeOptions);
    return `${startStr} - ${endStr}`;
  }
  return startStr;
};

const openLink = () => {
  if (props.event.url) {
    window.open(props.event.url, '_blank');
  }
};

const handleDelete = async () => {
  if (confirm('Are you sure you want to delete this event?')) {
    isDeleting.value = true;
    try {
      emit('delete', props.event.id, props.event.extendedProps?.calendarId);
    } catch (error) {
      console.error('Error in handleDelete:', error);
      showToast('Failed to delete event', 'error');
    } finally {
      isDeleting.value = false;
    }
  }
};
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2100;
}

.modal-container {
  width: 100%;
  max-width: 550px;
  border-radius: 8px;
  overflow: hidden;
}

.theme-bg-darker {
  background-color: var(--bg-darker);
}

.theme-border {
  border-color: var(--border-primary) !important;
}

.theme-text {
  color: var(--text-primary);
}

.theme-text-muted {
  color: var(--text-muted);
}
</style>
