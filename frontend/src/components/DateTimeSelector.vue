<template>
  <div class="datetime-selector">
    <div v-if="!isEditing" 
         class="display-value" 
         :class="[
           { 'display-value-sm': size === 'small', 'placeholder': !modelValue },
           getDateColorClass(modelValue, isClosed)
         ]"
         @click="startEditing">
      <span class="text-truncate flex-grow-1">{{ friendlyDisplay }}</span>
      <button v-if="modelValue" 
              class="inline-clear-btn"
              @click.stop="clearValue"
              @mousedown.stop
              title="Clear date">
        <i class="bi bi-x" style="pointer-events: none;"></i>
      </button>
    </div>
    <div v-else class="input-group" :class="{ 'input-group-sm': size === 'small' }" @mousedown.stop>
      <input 
        ref="textInputRef"
        type="text" 
        class="form-control" 
        :class="{ 'form-control-sm': size === 'small' }"
        v-model="textInput" 
        @blur="onBlur"
        @keydown.enter="handleEnter"
        @keydown.esc="cancelEditing"
        @input="onInput"
        :placeholder="placeholder"
      >
      <button v-if="modelValue" 
              class="btn btn-outline-secondary clear-btn"
              :class="{ 'btn-sm': size === 'small' }"
              type="button"
              @mousedown.stop.prevent="clearValue"
              @click.stop
              title="Clear date">
        <i class="bi bi-x"></i>
      </button>
      <button class="btn btn-outline-secondary calendar-btn" 
              :class="{ 'btn-sm': size === 'small' }"
              type="button" 
              @mousedown.prevent
              @click="triggerCalendar">
        <i class="bi bi-calendar3"></i>
      </button>
      <!-- Hidden native datetime-local to use its picker -->
      <input 
        type="datetime-local" 
        ref="datePicker" 
        class="hidden-picker" 
        v-model="pickerValue"
        @change="onPickerChange"
      >
    </div>
    <div v-if="isEditing && (parseError || previewDate)" class="feedback-container">
      <div v-if="parseError" class="parse-error">
        {{ parseError }}
      </div>
      <div v-else-if="previewDate" class="preview-date">
        {{ previewDate }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, computed, nextTick } from 'vue';
import { formatForInput, formatToISO, formatFriendlyDate, getDateColorClass } from '../js/utils';

const props = defineProps({
  modelValue: String,
  placeholder: String,
  defaultTime: {
    type: String,
    default: '23:59'
  },
  size: {
    type: String,
    default: 'medium' // 'small' or 'medium'
  },
  isClosed: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['update:modelValue']);

const textInput = ref('');
const pickerValue = ref('');
const datePicker = ref(null);
const textInputRef = ref(null);
const parseError = ref('');
const internalValue = ref(props.modelValue);
const isEditing = ref(false);
const lastClearTime = ref(0);

const isActuallyEmpty = (val) => val === null || val === undefined || val === '';

const friendlyDisplay = computed(() => {
  if (isActuallyEmpty(props.modelValue)) return props.placeholder || '-';
  return formatFriendlyDate(props.modelValue, false, true);
});

const startEditing = () => {
  if (isEditing.value) return;
  isEditing.value = true;
  // If we recently cleared it, internalValue is null. 
  // We should use that instead of the potentially stale props.modelValue
  updateTextInput(internalValue.value);
  nextTick(() => {
    if (textInputRef.value) {
      textInputRef.value.focus();
    }
  });
};

const cancelEditing = () => {
  isEditing.value = false;
  parseError.value = '';
};

const handleEnter = () => {
  commitEdit();
};

const commitEdit = () => {
  if (isActuallyEmpty(textInput.value)) {
    clearValue();
    return;
  }

  const parsed = parseDate(textInput.value);
  if (parsed) {
    const iso = parsed.toISOString();
    if (iso !== props.modelValue) {
      console.log('[DateTimeSelector] commitEdit: new value', iso);
      internalValue.value = iso;
      lastClearTime.value = 0; // Reset clear suppression if we manually set a date
      emit('update:modelValue', iso);
    }
    updateTextInput(iso);
    parseError.value = '';
    isEditing.value = false;
  } else {
    // Try native Date parsing
    const d = new Date(textInput.value);
    if (!isNaN(d.getTime())) {
      const iso = d.toISOString();
      if (iso !== props.modelValue) {
        console.log('[DateTimeSelector] commitEdit (native): new value', iso);
        internalValue.value = iso;
        lastClearTime.value = 0; // Reset clear suppression
        emit('update:modelValue', iso);
      }
      updateTextInput(iso);
      parseError.value = '';
      isEditing.value = false;
    } else {
      parseError.value = 'Could not parse date';
    }
  }
};

const clearValue = () => {
  internalValue.value = null;
  textInput.value = '';
  pickerValue.value = '';
  lastClearTime.value = Date.now();
  emit('update:modelValue', null);
  // Using a timeout for isEditing to prevent immediate blur->commit
  setTimeout(() => {
    isEditing.value = false;
  }, 50);
  parseError.value = '';
};

// Initialize text input from modelValue
const updateTextInput = (val) => {
  if (isActuallyEmpty(val)) {
    textInput.value = '';
    pickerValue.value = '';
    return;
  }
  const date = new Date(val);
  if (!isNaN(date.getTime())) {
    // Check if it's the exact end of day (23:59:59)
    const isEOD = date.getHours() === 23 && date.getMinutes() === 59;
    
    const options = { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric'
    };
    
    if (!isEOD) {
      options.hour = '2-digit';
      options.minute = '2-digit';
    }

    textInput.value = date.toLocaleString([], options);
    pickerValue.value = formatForInput(val);
  } else {
    textInput.value = '';
    pickerValue.value = '';
  }
};

watch(() => props.modelValue, (newVal) => {
  const timeSinceClear = Date.now() - lastClearTime.value;
  const isSuppressing = lastClearTime.value > 0 && timeSinceClear < 5000;

  if (newVal !== internalValue.value) {
    // If both are empty, just sync internal state without re-updating text input
    if (isActuallyEmpty(internalValue.value) && isActuallyEmpty(newVal)) {
      internalValue.value = newVal;
      return;
    }
    
    // If we just cleared it (within the last 5 seconds), don't let a non-empty newVal restore it.
    // This handles the race condition where the parent emits the old value while the API call is in progress.
    if (isActuallyEmpty(internalValue.value) && !isActuallyEmpty(newVal) && isSuppressing) {
      // console.log(`[DateTimeSelector] Ignoring stale prop update (${Math.round(timeSinceClear)}ms after clear):`, newVal);
      return;
    }
    
    // console.log('[DateTimeSelector] Prop update accepted:', newVal);
    internalValue.value = newVal;
    updateTextInput(newVal);
    
    // If we received a non-empty value that matches our internal expectation (or we are not suppressing),
    // we can stop suppressing further updates.
    if (!isActuallyEmpty(newVal) || !isSuppressing) {
      lastClearTime.value = 0;
    }
  }
}, { immediate: true });

const previewDate = computed(() => {
  if (!textInput.value || parseError.value) return '';
  const parsed = parseDate(textInput.value);
  if (parsed) {
    // Check if it's the exact end of day (23:59:59)
    const isEOD = parsed.getHours() === 23 && parsed.getMinutes() === 59;
    
    const options = { 
      weekday: 'short',
      year: 'numeric', 
      month: 'short', 
      day: 'numeric'
    };
    
    if (!isEOD) {
      options.hour = '2-digit';
      options.minute = '2-digit';
    }

    return parsed.toLocaleString([], options);
  }
  return '';
});

const onInput = () => {
  parseError.value = '';
};

const onBlur = (event) => {
  // Use a small timeout to allow calendar button clicks to process before hiding
  setTimeout(() => {
    // If the component was already closed (e.g. by clearValue), do nothing
    if (!isEditing.value) return;

    // Check if focus is still within the component
    // If we're clicking the clear button or calendar button, we might briefly lose focus but stay in selector
    if (document.activeElement && 
        (document.activeElement === textInputRef.value || 
         document.activeElement.closest('.datetime-selector') === textInputRef.value?.closest('.datetime-selector'))) {
      return;
    }

    commitEdit();
  }, 200);
};

const triggerCalendar = () => {
  if (datePicker.value) {
    datePicker.value.showPicker();
  }
};

const onPickerChange = () => {
  if (pickerValue.value) {
    const date = new Date(pickerValue.value);
    const iso = date.toISOString();
    if (iso !== props.modelValue) {
      internalValue.value = iso;
      emit('update:modelValue', iso);
    }
    updateTextInput(iso);
    isEditing.value = false;
  } else {
    clearValue();
  }
};

const parseDate = (input) => {
  const str = input.toLowerCase().trim();
  if (!str) return null;

  const now = new Date();
  let result = new Date(now);
  
  // Default to EOD if time not specified
  let timeSet = false;

  // Check for relative words
  if (str.includes('today')) {
    // result is already today
  } else if (str.includes('tomorrow')) {
    result.setDate(now.getDate() + 1);
  } else if (str.includes('next week')) {
    // Move to next Monday
    const day = now.getDay();
    const diff = (day === 0 ? 1 : 8 - day);
    result.setDate(now.getDate() + diff);
  } else if (str.includes('next month')) {
    result.setMonth(now.getMonth() + 1);
    result.setDate(1);
  } else {
    // Check for weekdays
    const weekdays = ['sunday', 'monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday'];
    let dayIndex = -1;
    for (let i = 0; i < weekdays.length; i++) {
      if (str.includes(weekdays[i])) {
        dayIndex = i;
        break;
      }
    }

    if (dayIndex !== -1) {
      const currentDay = now.getDay();
      let diff = dayIndex - currentDay;
      if (diff <= 0) diff += 7;
      result.setDate(now.getDate() + diff);
    } else {
      // Try native Date parsing
      const d = new Date(input);
      if (!isNaN(d.getTime())) {
        // If the input was something like "2026-07-10" (no time), 
        // d might be UTC or local midnight.
        // If the original input didn't have a colon (time), we might want to apply EOD.
        if (!str.includes(':')) {
           d.setHours(23, 59, 59, 999);
        }
        return d;
      }
      return null;
    }
  }

  // Extract time if present (e.g., "tomorrow 10:00", "tomorrow 10am")
  const timeMatch = str.match(/(\d{1,2})(?::(\d{2}))?\s*(am|pm)?/);
  if (timeMatch && (str.includes(':') || str.includes('am') || str.includes('pm'))) {
    let hours = parseInt(timeMatch[1]);
    const minutes = timeMatch[2] ? parseInt(timeMatch[2]) : 0;
    const ampm = timeMatch[3];

    if (ampm === 'pm' && hours < 12) hours += 12;
    if (ampm === 'am' && hours === 12) hours = 0;
    
    result.setHours(hours, minutes, 0, 0);
    timeSet = true;
  }

  if (!timeSet && !str.includes(':')) {
    const [defH, defM] = props.defaultTime.split(':').map(Number);
    result.setHours(defH, defM, 59, 999);
  } else if (!timeSet) {
    // If no explicit time found by regex but might be there (e.g. from native parsing)
    // we already returned from native parsing branch or handled it.
  }

  return result;
};
</script>

<style scoped>
.datetime-selector {
  position: relative;
  width: 100%;
  min-height: 31px;
  display: flex;
  align-items: center;
}

.display-value {
  width: 100%;
  padding: 4px 8px;
  cursor: pointer;
  border-radius: 4px;
  border: 1px solid transparent;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  color: var(--text-primary);
  transition: all 0.2s;
  background-color: transparent;
  display: flex;
  align-items: center;
}

.inline-clear-btn {
  background: none;
  border: none;
  color: var(--text-muted);
  padding: 0 2px;
  margin-left: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  opacity: 0;
  transition: opacity 0.2s, background-color 0.2s;
}

.display-value:hover .inline-clear-btn {
  opacity: 1;
}

.inline-clear-btn:hover {
  background-color: var(--bg-card);
  color: var(--text-primary);
}

.display-value:hover {
  background-color: var(--bg-dark);
  border-color: var(--border-primary);
}

.display-value-sm {
  padding: 2px 6px;
  font-size: 0.875rem;
}

.display-value.placeholder {
  color: var(--text-muted);
  opacity: 0.7;
}

.date-muted {
  color: var(--text-muted) !important;
  opacity: 0.7;
}

.date-overdue {
  color: #ff4d4d !important;
  font-weight: 500;
}

.date-today {
  color: #ffcc00 !important;
}

.date-this-week {
  color: #3399ff !important;
}

.hidden-picker {
  position: absolute;
  visibility: hidden;
  width: 0;
  height: 0;
  padding: 0;
  border: none;
}

.calendar-btn, .clear-btn {
  border-top-left-radius: 0;
  border-bottom-left-radius: 0;
  background-color: var(--bg-darker);
  border-color: var(--border-primary);
  color: var(--text-muted);
}

.clear-btn {
  border-right: none;
}

.calendar-btn:hover, .clear-btn:hover {
  background-color: var(--bg-card);
  color: var(--text-primary);
}

.feedback-container {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  z-index: 2000;
  background-color: var(--bg-dark);
  border: 1px solid var(--border-primary);
  border-top: none;
  border-bottom-left-radius: 4px;
  border-bottom-right-radius: 4px;
  padding: 2px 8px;
  box-shadow: 0 4px 6px rgba(0,0,0,0.3);
  pointer-events: none;
  width: max-content;
  min-width: 100%;
}

.parse-error {
  color: #f44747;
  font-size: 0.75rem;
}

.preview-date {
  color: var(--text-muted);
  font-size: 0.75rem;
}

.input-group .form-control {
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
  background-color: var(--bg-darker);
  border-color: var(--border-primary);
  color: var(--text-primary);
}

.input-group .form-control:focus {
  background-color: var(--bg-dark);
  border-color: var(--accent-blue);
  box-shadow: none;
}
</style>
