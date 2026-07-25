<template>
  <div class="card bg-dark text-light border-secondary">
    <div class="card-header border-secondary d-flex justify-content-between align-items-center">
      <h5 class="mb-0 small text-muted text-uppercase fw-bold">Templates</h5>
      <div class="d-flex gap-2">
        <button v-for="preset in presets" :key="preset.name" 
                class="btn btn-sm btn-outline-secondary preset-btn"
                @click="applyPreset(preset)"
                :title="'Apply ' + preset.name + ' preset'">
          {{ preset.name }}
        </button>
      </div>
    </div>
    <div class="card-body">
      <div v-if="!colorSchemes || colorSchemes.length === 0" class="p-4 text-center text-muted">
        No color schemes created yet.
      </div>
      
      <div class="row g-3">
        <div v-for="(scheme, schemeIdx) in colorSchemes" :key="schemeIdx" class="col-md-6">
          <div class="card bg-dark border-secondary h-100">
            <div class="card-header border-secondary p-2 d-flex justify-content-between align-items-center bg-secondary bg-opacity-10">
              <input v-model="scheme.name" type="text" class="form-control form-control-sm bg-transparent text-light border-0 fw-bold p-0" placeholder="Scheme Name">
              <button class="btn btn-link btn-sm text-danger p-0 ms-2" @click="removeScheme(schemeIdx)">
                <i class="bi bi-trash"></i>
              </button>
            </div>
            <div class="card-body p-2">
              <div v-for="(color, colorIdx) in scheme.colors" :key="colorIdx" class="d-flex align-items-center mb-2">
                <input v-model="color.name" type="text" class="form-control form-control-sm bg-dark text-light border-secondary me-2" style="flex: 2" placeholder="Color Name">
                <input v-model="color.color" type="color" class="form-control form-control-sm form-control-color bg-dark border-secondary me-2" style="width: 40px; padding: 2px;">
                
                <div class="dropdown me-2">
                  <button class="btn btn-sm btn-outline-info dropdown-toggle no-caret" type="button" data-bs-toggle="dropdown" title="Coordinate Colors">
                    <i class="bi bi-magic"></i>
                  </button>
                  <ul class="dropdown-menu dropdown-menu-dark border-secondary shadow">
                    <li><h6 class="dropdown-header">Add Coordinating Color</h6></li>
                    <li><a class="dropdown-item" href="#" @click.prevent="addCoordinated(scheme, color.color, 'complementary')">Complementary</a></li>
                    <li><a class="dropdown-item" href="#" @click.prevent="addCoordinated(scheme, color.color, 'analogous')">Analogous (2)</a></li>
                    <li><a class="dropdown-item" href="#" @click.prevent="addCoordinated(scheme, color.color, 'triadic')">Triadic (2)</a></li>
                  </ul>
                </div>

                <button class="btn btn-link btn-sm text-danger p-0" @click="removeColor(scheme, colorIdx)">
                  <i class="bi bi-dash-circle"></i>
                </button>
              </div>
              <button class="btn btn-sm btn-outline-info w-100 mt-1" @click="addColor(scheme)">
                <i class="bi bi-plus-sm"></i> Add Color
              </button>
            </div>
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
  }
];

const applyPreset = (preset) => {
  props.colorSchemes.push(JSON.parse(JSON.stringify(preset)));
};

const addScheme = () => {
  props.colorSchemes.push({
    name: 'New Scheme',
    colors: [
      { name: 'Primary', color: '#007bff' }
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
.form-control-color {
  min-width: 40px;
}

.no-caret::after {
  display: none !important;
}

.preset-btn {
  font-size: 0.75rem;
  padding: 0.1rem 0.5rem;
}
</style>
