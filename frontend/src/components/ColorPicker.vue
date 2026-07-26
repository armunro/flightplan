<template>
  <div class="color-picker-component" :class="containerClass" ref="pickerRef">
    <div class="d-flex flex-column gap-2">
      <!-- Selected Color and Toggle -->
      <div class="d-flex align-items-center gap-2">
        <div 
          class="color-swatch-wrapper" 
          :style="swatchStyle"
          @click="showPalette = !showPalette"
          :title="showPalette ? 'Close palette' : 'Open palette'"
        >
          <div class="selected-color-indicator" :style="{ backgroundColor: modelValue }"></div>
        </div>
        
        <input 
          v-if="showText"
          type="text" 
          :value="modelValue" 
          @input="$emit('update:modelValue', $event.target.value)"
          class="form-control form-control-sm color-text-input"
          placeholder="#000000"
        >
      </div>

      <div v-if="showPalette" class="color-palette-popover card bg-dark border-secondary shadow p-2" :style="paletteStyle">
        <div class="color-grid">
          <button
            v-for="color in standardColors"
            :key="color"
            class="color-cell"
            :class="{ active: modelValue.toLowerCase() === color.toLowerCase() }"
            :style="{ backgroundColor: color }"
            @click="selectColor(color)"
            :title="color"
          ></button>
        </div>
        <div class="palette-footer mt-2 pt-2 border-top border-secondary d-flex justify-content-between align-items-center">
          <small class="text-muted">Standard Colors</small>
          <button class="btn btn-sm btn-link text-info p-0" @click="showPalette = false">Close</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';

const props = defineProps({
  modelValue: {
    type: String,
    default: '#000000'
  },
  showText: {
    type: Boolean,
    default: false
  },
  size: {
    type: String,
    default: 'md' // 'sm', 'md', 'lg'
  },
  containerClass: {
    type: String,
    default: ''
  },
  palettePlacement: {
    type: String,
    default: 'bottom-start' // 'bottom-start', 'bottom-end', 'top-start', 'top-end'
  }
});

const emit = defineEmits(['update:modelValue']);

const showPalette = ref(false);
const pickerRef = ref(null);

const handleClickOutside = (event) => {
  if (showPalette.value && pickerRef.value && !pickerRef.value.contains(event.target)) {
    showPalette.value = false;
  }
};

import { onMounted, onUnmounted } from 'vue';
onMounted(() => document.addEventListener('click', handleClickOutside));
onUnmounted(() => document.removeEventListener('click', handleClickOutside));

const paletteStyle = computed(() => {
  const style = {};
  if (props.palettePlacement.startsWith('top')) {
    style.bottom = '100%';
    style.top = 'auto';
    style.marginBottom = '5px';
  } else {
    style.top = '100%';
    style.bottom = 'auto';
    style.marginTop = '5px';
  }
  
  if (props.palettePlacement.endsWith('end')) {
    style.right = '0';
    style.left = 'auto';
  } else {
    style.left = '0';
    style.right = 'auto';
  }
  return style;
});

const standardColors = [
  // Row 1: Grayscale/Dark
  '#212529', '#495057', '#adb5bd', '#dee2e6', '#f8f9fa',
  // Row 2: Reds/Pinks
  '#dc3545', '#e83e8c', '#fd7e14', '#ffc107', '#ffca2c',
  // Row 3: Greens
  '#198754', '#20c997', '#0dcaf0', '#0ea5e9', '#0d6efd',
  // Row 4: Blues/Purples
  '#58a6ff', '#0a58ca', '#6610f2', '#6f42c1', '#d63384'
];

const selectColor = (color) => {
  emit('update:modelValue', color);
  showPalette.value = false;
};

const swatchSize = computed(() => {
  switch (props.size) {
    case 'sm': return '24px';
    case 'lg': return '38px';
    default: return '32px';
  }
});

const swatchStyle = computed(() => ({
  width: swatchSize.value,
  height: swatchSize.value,
  borderRadius: '4px',
  border: '1px solid var(--bs-border-color)',
  position: 'relative',
  cursor: 'pointer',
  flexShrink: 0,
  padding: '2px',
  background: 'var(--bs-dark)'
}));
</script>

<style scoped>
.color-picker-component {
  position: relative;
}

.selected-color-indicator {
  width: 100%;
  height: 100%;
  border-radius: 2px;
}

.color-text-input {
  font-family: monospace;
  max-width: 100px;
}

.color-palette-popover {
  position: absolute;
  z-index: 1060;
  width: 180px;
}

.color-grid {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 4px;
}

.color-cell {
  width: 28px;
  height: 28px;
  border-radius: 4px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  padding: 0;
  cursor: pointer;
  transition: transform 0.1s;
}

.color-cell:hover {
  transform: scale(1.1);
  border-color: rgba(255, 255, 255, 0.5);
  z-index: 1;
}

.color-cell.active {
  border: 2px solid white;
  box-shadow: 0 0 0 1px black;
}

.palette-footer {
  font-size: 0.75rem;
}
</style>
