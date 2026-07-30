<template>
  <div class="color-schemes-page">
    <div class="card theme-card mb-4 shadow-sm border-0 overflow-hidden">
      <div class="card-header theme-border py-3 bg-secondary bg-opacity-10">
        <h5 class="mb-0 small theme-text-muted text-uppercase fw-bold tracking-wider">Presets</h5>
      </div>
      <div class="card-body p-3">
        <div class="d-flex flex-wrap gap-2">
          <button v-for="preset in presets" :key="preset.name" 
                  class="btn btn-sm btn-outline-secondary preset-btn px-3 rounded-pill"
                  @click="applyPreset(preset)"
                  :title="'Apply ' + preset.name + ' preset'">
            {{ preset.name }}
          </button>
        </div>
      </div>
    </div>
    
    <div v-if="!colorSchemes || colorSchemes.length === 0" class="p-5 text-center theme-card border rounded shadow-sm">
      <i class="bi bi-palette theme-text-muted mb-3 d-block" style="font-size: 3rem;"></i>
      <h5 class="theme-text">No color schemes yet</h5>
      <p class="theme-text-muted mb-4">Choose a preset above or create a new one from scratch.</p>
    </div>
    
    <div class="accordion color-schemes-accordion" id="colorSchemesAccordion">
      <div v-for="(scheme, schemeIdx) in colorSchemes" :key="schemeIdx" class="accordion-item theme-card mb-2 border-0 overflow-hidden shadow-sm">
        <h2 class="accordion-header">
          <button class="accordion-button collapsed py-3 theme-text bg-transparent d-flex align-items-center" 
                  type="button" 
                  data-bs-toggle="collapse" 
                  :data-bs-target="'#collapse' + schemeIdx" 
                  aria-expanded="false" 
                  :aria-controls="'#collapse' + schemeIdx">
            <div class="scheme-preview-mini me-3">
              <div v-for="(c, i) in scheme.colors.slice(0, 4)" :key="i" 
                   :style="{ backgroundColor: c.color }" 
                   class="preview-dot"></div>
            </div>
            <span class="flex-grow-1 fw-bold">{{ scheme.name || 'Unnamed Scheme' }}</span>
            <span class="badge rounded-pill bg-secondary bg-opacity-25 theme-text-muted me-3">{{ scheme.colors.length }} Colors</span>
          </button>
        </h2>
        <div :id="'collapse' + schemeIdx" class="accordion-collapse collapse" data-bs-parent="#colorSchemesAccordion">
          <div class="accordion-body theme-border-top p-3">
            <div class="d-flex gap-3 align-items-center mb-3">
              <div class="flex-grow-1">
                <label class="form-label small theme-text-muted text-uppercase fw-bold">Scheme Name</label>
                <input v-model="scheme.name" type="text" class="form-control theme-input" placeholder="Scheme Name">
              </div>
              <div class="pt-4">
                <button class="btn btn-outline-danger" @click="removeScheme(schemeIdx)">
                  <i class="bi bi-trash me-2"></i>Delete Scheme
                </button>
              </div>
            </div>

            <div class="color-list mt-3">
              <div class="d-flex align-items-center mb-2 px-2 py-1 theme-text-muted small text-uppercase fw-bold border-bottom theme-border">
                <div style="width: 40px" class="me-3">Color</div>
                <div class="flex-grow-1">Name</div>
                <div style="width: 120px">HEX</div>
                <div style="width: 80px" class="text-end">Actions</div>
              </div>

              <div v-for="(color, colorIdx) in scheme.colors" :key="colorIdx" class="color-row d-flex align-items-center p-2 rounded transition hover-bg">
                <div class="color-picker-wrapper me-3">
                  <input v-model="color.color" type="color" 
                         class="form-control-color border-0 p-0 rounded-circle shadow-sm" 
                         style="width: 32px; height: 32px;">
                </div>

                <div class="flex-grow-1">
                  <input v-model="color.name" type="text" 
                         class="form-control form-control-sm border-0 bg-transparent theme-text fw-medium p-0" 
                         placeholder="Color Name">
                </div>

                <div style="width: 120px" class="font-monospace small opacity-75">
                  {{ color.color.toUpperCase() }}
                </div>
                
                <div style="width: 80px" class="d-flex gap-1 justify-content-end align-items-center">
                  <div class="dropdown">
                    <button class="btn btn-sm btn-link text-info p-1 no-caret" type="button" data-bs-toggle="dropdown" title="Coordinate Colors">
                      <i class="bi bi-magic"></i>
                    </button>
                    <ul class="dropdown-menu border-0 shadow-lg dropdown-menu-end" :class="{ 'dropdown-menu-dark': config && config.theme === 'Cosmic' }">
                      <li><h6 class="dropdown-header">Add Coordinating Color</h6></li>
                      <li><a class="dropdown-item" href="#" @click.prevent="addCoordinated(scheme, color.color, 'complementary')">
                        <i class="bi bi-circle-half me-2"></i>Complementary
                      </a></li>
                      <li><a class="dropdown-item" href="#" @click.prevent="addCoordinated(scheme, color.color, 'analogous')">
                        <i class="bi bi-intersect me-2"></i>Analogous (2)
                      </a></li>
                      <li><a class="dropdown-item" href="#" @click.prevent="addCoordinated(scheme, color.color, 'triadic')">
                        <i class="bi bi-triangle me-2"></i>Triadic (2)
                      </a></li>
                    </ul>
                  </div>

                  <button class="btn btn-sm btn-link text-danger p-1" @click="removeColor(scheme, colorIdx)" title="Remove color">
                    <i class="bi bi-dash-circle"></i>
                  </button>
                </div>
              </div>
            </div>
            
            <button class="btn btn-sm btn-subtle w-100 mt-3 py-2 d-flex align-items-center justify-content-center gap-2 border-dashed" @click="addColor(scheme)">
              <i class="bi bi-plus-lg"></i> Add Color to Scheme
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { getComplementary, getAnalogous, getTriadic } from '../js/colorUtils.js';

const props = defineProps({
  colorSchemes: {
    type: Array,
    required: true
  },
  config: {
    type: Object,
    required: false
  }
});

const presets = [
  {
    name: 'Ocean',
    colors: [
      { name: 'Deep Sea', color: '#002B5B' },
      { name: 'Marine', color: '#2B4865' },
      { name: 'Teal', color: '#256D85' },
      { name: 'Aqua', color: '#8FE3CF' }
    ]
  },
  {
    name: 'Sunset',
    colors: [
      { name: 'Midnight', color: '#2D033B' },
      { name: 'Deep Purple', color: '#810CA8' },
      { name: 'Magenta', color: '#C147E9' },
      { name: 'Peach', color: '#E5B8F4' }
    ]
  },
  {
    name: 'Forest',
    colors: [
      { name: 'Pine', color: '#1B4D3E' },
      { name: 'Fern', color: '#397D54' },
      { name: 'Moss', color: '#7FB069' },
      { name: 'Sage', color: '#D3E4CD' }
    ]
  },
  {
    name: 'Corporate',
    colors: [
      { name: 'Primary', color: '#007BFF' },
      { name: 'Secondary', color: '#6C757D' },
      { name: 'Success', color: '#28A745' },
      { name: 'Info', color: '#17A2B8' }
    ]
  },
  {
    name: 'Midnight',
    colors: [
      { name: 'Blackout', color: '#0F172A' },
      { name: 'Slate', color: '#1E293B' },
      { name: 'Blue Gray', color: '#334155' },
      { name: 'Light Slate', color: '#64748B' }
    ]
  },
  {
    name: 'Rainbow',
    colors: [
      { name: 'Red', color: '#FF5F5F' },
      { name: 'Orange', color: '#FFA45E' },
      { name: 'Yellow', color: '#FFD75E' },
      { name: 'Green', color: '#7AFF5E' },
      { name: 'Blue', color: '#5EBDFF' },
      { name: 'Indigo', color: '#8E5EFF' },
      { name: 'Violet', color: '#E15EFF' }
    ]
  },
  {
    name: 'Vibrant',
    colors: [
      { name: 'Electric Blue', color: '#00D2FF' },
      { name: 'Vivid Pink', color: '#FF007A' },
      { name: 'Bright Orange', color: '#FF9500' },
      { name: 'Lime', color: '#ADFF2F' }
    ]
  },
  {
    name: 'Pastel',
    colors: [
      { name: 'Dusty Rose', color: '#F2C6C2' },
      { name: 'Muted Sage', color: '#C2D2BD' },
      { name: 'Soft Periwinkle', color: '#C6CBEF' },
      { name: 'Pale Gold', color: '#F2E2C2' }
    ]
  },
  {
    name: 'Earth',
    colors: [
      { name: 'Terracotta', color: '#E2725B' },
      { name: 'Olive', color: '#808000' },
      { name: 'Sand', color: '#C2B280' },
      { name: 'Slate', color: '#708090' }
    ]
  },
  {
    name: 'Cyberpunk',
    colors: [
      { name: 'Neon Purple', color: '#B026FF' },
      { name: 'Cyan', color: '#00FFFF' },
      { name: 'Hot Pink', color: '#FF69B4' },
      { name: 'Bright Yellow', color: '#FFFF00' }
    ]
  }
];

const applyPreset = (preset) => {
  props.colorSchemes.push(JSON.parse(JSON.stringify(preset)));
};

const addScheme = () => {
  props.colorSchemes.push({
    name: 'New Scheme',
    colors: [
      { name: 'Primary', color: '#58a6ff' }
    ]
  });
};

const removeScheme = (index) => {
  if (confirm('Are you sure you want to delete this color scheme?')) {
    props.colorSchemes.splice(index, 1);
  }
};

const addColor = (scheme) => {
  scheme.colors.push({
    name: 'New Color',
    color: '#ffffff'
  });
};

const removeColor = (scheme, index) => {
  scheme.colors.splice(index, 1);
};

const addCoordinated = (scheme, baseColor, type) => {
  if (type === 'complementary') {
    scheme.colors.push({ name: 'Complementary', color: getComplementary(baseColor) });
  } else if (type === 'analogous') {
    const colors = getAnalogous(baseColor);
    scheme.colors.push({ name: 'Analogous 1', color: colors[0] });
    scheme.colors.push({ name: 'Analogous 2', color: colors[1] });
  } else if (type === 'triadic') {
    const colors = getTriadic(baseColor);
    scheme.colors.push({ name: 'Triadic 1', color: colors[0] });
    scheme.colors.push({ name: 'Triadic 2', color: colors[1] });
  }
};
</script>

<style scoped>
.color-schemes-page {
  animation: fadeIn 0.3s ease-in-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.color-schemes-accordion .accordion-button {
  box-shadow: none !important;
}

.color-schemes-accordion .accordion-button::after {
  filter: brightness(0) invert(1);
}

.color-schemes-accordion .accordion-item {
  border: 1px solid rgba(255, 255, 255, 0.05) !important;
}

.theme-border-top {
  border-top: 1px solid var(--border-primary);
}

.scheme-card {
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  border: 1px solid rgba(255, 255, 255, 0.05) !important;
}

.scheme-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.2) !important;
}

.scheme-name-input {
  font-size: 1.25rem;
  outline: none !important;
  box-shadow: none !important;
}

.scheme-preview-mini {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 2px;
  width: 32px;
  height: 32px;
  border-radius: 6px;
  overflow: hidden;
  background: rgba(0, 0, 0, 0.1);
}

.preview-dot {
  width: 100%;
  height: 100%;
}

.color-row {
  border: 1px solid transparent;
}

.hover-bg:hover {
  background-color: rgba(255, 255, 255, 0.03);
  border-color: rgba(255, 255, 255, 0.05);
}

.form-control-color {
  cursor: pointer;
  background: none;
  transition: transform 0.1s ease;
}

.form-control-color:hover {
  transform: scale(1.1);
}

.btn-icon-only {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  background: none;
  border: none;
}

.btn-icon-only:hover {
  background-color: rgba(220, 53, 69, 0.1);
}

.transition {
  transition: all 0.2s ease;
}

.hover-opacity-100:hover {
  opacity: 1 !important;
}

.border-dashed {
  border: 2px dashed rgba(255, 255, 255, 0.1) !important;
  background: transparent !important;
}

.border-dashed:hover {
  border-color: var(--accent-blue) !important;
  color: var(--accent-blue) !important;
  background: rgba(88, 166, 255, 0.05) !important;
}

.btn-subtle {
  color: var(--text-muted);
  font-weight: 500;
}

.tracking-wider {
  letter-spacing: 0.05em;
}

.no-caret::after {
  display: none !important;
}

.preset-btn {
  font-size: 0.85rem;
  border-color: rgba(255, 255, 255, 0.1);
}

.preset-btn:hover {
  background-color: var(--accent-blue);
  border-color: var(--accent-blue);
  color: white;
}
</style>
