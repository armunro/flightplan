<template>
  <div class="card bg-dark text-light border-secondary">
    <div class="card-header border-secondary d-flex justify-content-between align-items-center">
      <h5 class="mb-0"><i class="bi bi-palette me-2"></i>Color Schemes</h5>
      <button class="btn btn-sm btn-success" @click="addScheme">
        <i class="bi bi-plus-lg me-1"></i>Add Scheme
      </button>
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
const props = defineProps({
  colorSchemes: {
    type: Array,
    required: true
  }
});

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
</script>

<style scoped>
.form-control-color {
  min-width: 40px;
}
</style>
