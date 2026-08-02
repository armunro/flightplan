<template>
  <div v-if="isOpen" class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-container theme-bg-darker theme-border theme-shadow">
      <div class="modal-header d-flex align-items-center justify-content-between p-3 border-bottom theme-border">
        <h5 class="mb-0 theme-text">{{ isEdit ? 'Edit Event' : 'Add Event' }}</h5>
        <button class="btn-close btn-close-white" @click="$emit('close')"></button>
      </div>
      
      <div class="modal-body p-3">
        <form @submit.prevent="handleSubmit">
          <div class="mb-3">
            <label class="form-label theme-text-muted">Subject</label>
            <input 
              v-model="form.subject" 
              type="text" 
              class="form-control theme-input" 
              placeholder="Event title"
              required
              ref="subjectInput"
            />
          </div>
          
          <div class="mb-3">
            <label class="form-label theme-text-muted">Calendar</label>
            <select v-model="form.calendarId" class="form-select theme-input">
              <option v-for="cal in calendars" :key="cal.id" :value="cal.id">
                {{ cal.displayName }}
              </option>
            </select>
          </div>

          <div class="row mb-3">
            <div class="col-md-6">
              <label class="form-label theme-text-muted">Start</label>
              <DateTimeSelector 
                v-model="form.start" 
                :is-all-day="form.isAllDay"
              />
            </div>
            <div class="col-md-6">
              <label class="form-label theme-text-muted">End</label>
              <DateTimeSelector 
                v-model="form.end" 
                :is-all-day="form.isAllDay"
              />
            </div>
          </div>

          <div class="mb-3 form-check">
            <input 
              type="checkbox" 
              class="form-check-input theme-checkbox" 
              id="isAllDay" 
              v-model="form.isAllDay"
            />
            <label class="form-check-label theme-text" for="isAllDay">All Day Event</label>
          </div>

          <div class="mb-3">
            <label class="form-label theme-text-muted">Location</label>
            <input 
              v-model="form.location" 
              type="text" 
              class="form-control theme-input" 
              placeholder="Add location"
            />
          </div>

          <div class="d-flex justify-content-end gap-2 mt-4">
            <button type="button" class="btn btn-subtle" @click="$emit('close')">Cancel</button>
            <button type="submit" class="btn btn-primary" :disabled="isSubmitting || !form.subject">
              <span v-if="isSubmitting" class="spinner-border spinner-border-sm me-1"></span>
              {{ isEdit ? 'Update Event' : 'Create Event' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, watch, onMounted, computed } from 'vue';
import DateTimeSelector from './DateTimeSelector.vue';
import { addCalendarEvent, updateCalendarEvent } from '../js/calendar-api';
import { showToast } from './Toast.vue';

const props = defineProps({
  isOpen: Boolean,
  calendars: {
    type: Array,
    default: () => []
  },
  initialData: {
    type: Object,
    default: () => ({})
  }
});

const emit = defineEmits(['close', 'eventAdded']);

const subjectInput = ref(null);
const isSubmitting = ref(false);

const isEdit = computed(() => !!props.initialData?.id);

const form = reactive({
  subject: '',
  calendarId: 'primary',
  start: new Date(),
  end: new Date(new Date().getTime() + 60 * 60 * 1000), // +1 hour
  location: '',
  isAllDay: false
});

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    form.subject = props.initialData.subject || '';
    form.calendarId = props.initialData.calendarId || (props.calendars.length > 0 ? props.calendars[0].id : 'primary');
    form.start = props.initialData.start ? new Date(props.initialData.start) : new Date();
    form.end = props.initialData.end ? new Date(props.initialData.end) : new Date(form.start.getTime() + 60 * 60 * 1000);
    form.location = props.initialData.location || '';
    form.isAllDay = props.initialData.isAllDay || false;

    setTimeout(() => {
      subjectInput.value?.focus();
    }, 100);
  }
});

const handleSubmit = async () => {
  if (!form.subject) return;
  
  isSubmitting.value = true;
  try {
    const eventData = {
      subject: form.subject,
      calendarId: form.calendarId,
      start: typeof form.start === 'string' ? form.start : form.start.toISOString(),
      end: typeof form.end === 'string' ? form.end : form.end.toISOString(),
      location: form.location,
      isAllDay: form.isAllDay
    };
    
    if (isEdit.value) {
      await updateCalendarEvent(props.initialData.id, eventData, form.calendarId);
      showToast('Event updated successfully', 'success');
    } else {
      await addCalendarEvent(eventData);
      showToast('Event created successfully', 'success');
    }
    emit('eventAdded');
    emit('close');
  } catch (error) {
    const action = isEdit.value ? 'updating' : 'creating';
    console.error(`Error ${action} event:`, error);
    showToast(`Failed to ${isEdit.value ? 'update' : 'create'} event: ` + error.message, 'error');
  } finally {
    isSubmitting.value = false;
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
  z-index: 2000;
}

.modal-container {
  width: 100%;
  max-width: 500px;
  border-radius: 8px;
  overflow: hidden;
}

.theme-input, .theme-select {
  background-color: var(--bg-dark);
  border-color: var(--border-primary);
  color: var(--text-primary);
}

.theme-input:focus {
  background-color: var(--bg-dark);
  border-color: var(--accent-blue);
  color: var(--text-primary);
  box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
}
</style>
