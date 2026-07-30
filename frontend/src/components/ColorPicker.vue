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
        <!-- Tabs for Standard vs Schemes -->
        <div v-if="effectiveSchemes && effectiveSchemes.length > 0" class="picker-tabs sticky-top bg-dark mb-2" style="z-index: 10;">
          <div class="nav nav-pills nav-fill bg-secondary bg-opacity-10 rounded-1 p-1 gap-1">
            <button 
              class="nav-link py-1 px-2" 
              :class="{ 'active': activeTab === 'standard' }"
              @click="activeTab = 'standard'"
            >Standard</button>
            <button 
              class="nav-link py-1 px-2" 
              :class="{ 'active': activeTab === 'schemes' }"
              @click="activeTab = 'schemes'"
            >Schemes</button>
          </div>
        </div>

        <div v-if="activeTab === 'standard'" class="color-grid px-1">
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

        <div v-else-if="activeTab === 'schemes'" class="schemes-container px-1">
          <div v-if="effectiveSchemes.length === 0" class="p-2 text-center text-muted" style="font-size: 0.75rem;">
            No color schemes found. 
            <div class="mt-1">Add them in Settings.</div>
          </div>
          <div v-for="scheme in effectiveSchemes" :key="scheme.name" class="scheme-group mb-2">
            <div class="scheme-name mb-1">{{ scheme.name }}</div>
            <div class="color-grid">
              <button
                v-for="c in scheme.colors"
                :key="c.name + c.color"
                class="color-cell"
                :class="{ active: modelValue.toLowerCase() === c.color.toLowerCase() }"
                :style="{ backgroundColor: c.color }"
                @click="selectColor(c.color)"
                :title="c.name + ': ' + c.color"
              ></button>
            </div>
          </div>
        </div>

        <div class="palette-footer mt-2 pt-2 border-top border-secondary d-flex justify-content-between align-items-center px-1">
          <small class="text-muted">{{ activeTab === 'standard' ? 'Standard Colors' : 'Color Schemes' }}</small>
          <button class="btn btn-sm btn-link text-info p-0" @click="showPalette = false">Close</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { fetchSettings } from '../js/dashboard-api';

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
  },
  colorSchemes: {
    type: Array,
    default: null
  }
});

const emit = defineEmits(['update:modelValue']);

const showPalette = ref(false);
const pickerRef = ref(null);
const activeTab = ref('standard');
const fetchedSchemes = ref([]);

const effectiveSchemes = computed(() => {
  return props.colorSchemes || fetchedSchemes.value;
});

const handleClickOutside = (event) => {
  if (showPalette.value && pickerRef.value && !pickerRef.value.contains(event.target)) {
    showPalette.value = false;
  }
};

onMounted(async () => {
  document.addEventListener('click', handleClickOutside);
  
  if (!props.colorSchemes) {
    try {
      const config = await fetchSettings();
      if (config && config.colorSchemes) {
        fetchedSchemes.value = config.colorSchemes;
      }
    } catch (e) {
      console.error('Failed to fetch color schemes for ColorPicker:', e);
    }
  }
});

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
  border: 'none',
  position: 'relative',
  cursor: 'pointer',
  flexShrink: 0,
  padding: '0',
  background: 'transparent'
}));
</script>

<style scoped>
.color-picker-component {
  position: relative;
}

.selected-color-indicator {
  width: 100%;
  height: 100%;
  border-radius: 4px;
}

.color-text-input {
  font-family: monospace;
  max-width: 100px;
}

.color-palette-popover {
  position: absolute;
  z-index: 1060;
  width: 200px;
  max-height: 400px;
  overflow-y: auto;
  border-radius: 4px;
}

.picker-tabs {
  margin: -0.5rem -0.5rem 0.5rem -0.5rem;
  padding: 0.5rem;
  border-bottom: 1px solid var(--border-primary);
}

.nav-pills .nav-link {
  font-size: 0.7rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--text-muted);
  border-radius: 2px;
  transition: all 0.2s;
  border: none;
}

.nav-pills .nav-link:hover:not(.active) {
  background-color: rgba(255, 255, 255, 0.05);
  color: var(--text-primary);
}

.nav-pills .nav-link.active {
  background-color: var(--accent-blue);
  color: white;
  font-weight: bold;
}

.schemes-container {
  display: flex;
  flex-direction: column;
}

.scheme-name {
  font-size: 0.65rem;
  color: #6c757d;
  text-transform: uppercase;
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
