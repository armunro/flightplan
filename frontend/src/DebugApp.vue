<template>
  <div class="d-flex h-100">
    <Navbar />
    <div class="flex-grow-1 overflow-auto debug-container p-4">
      <div class="container-fluid">
        <h2 class="mb-4">Debug & Diagnostics</h2>
        
        <div v-if="loading" class="text-center my-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <div v-else-if="error" class="alert alert-danger">
          {{ error }}
        </div>

        <div v-else>
          <!-- Scheduler Information Section -->
          <section class="mb-5">
            <h4 class="border-bottom pb-2 mb-3">Quartz Scheduler Diagnostics</h4>
            <div class="row row-cols-1 row-cols-md-4 g-3">
              <div v-for="(value, key) in debugInfo.schedulerInfo" :key="key" class="col">
                <div class="card h-100 bg-dark border-secondary">
                  <div class="card-body py-2">
                    <div class="text-info x-small text-uppercase fw-bold">{{ formatKey(key) }}</div>
                    <div class="text-white small fw-bold">{{ value }}</div>
                  </div>
                </div>
              </div>
            </div>
          </section>

          <!-- Paths Section -->
          <section class="mb-5">
            <h4 class="border-bottom pb-2 mb-3">Important Paths</h4>
            <div class="table-responsive">
              <table class="table table-dark table-hover table-bordered">
                <thead>
                  <tr>
                    <th style="width: 250px">Key</th>
                    <th>Path</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(path, key) in debugInfo.paths" :key="key">
                    <td class="text-info">{{ formatKey(key) }}</td>
                    <td class="font-monospace">{{ path }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <!-- System Information Section -->
          <section class="mb-5">
            <h4 class="border-bottom pb-2 mb-3">System Information</h4>
            <div class="row row-cols-1 row-cols-md-2 g-4">
              <div v-for="(value, key) in debugInfo.systemInfo" :key="key" class="col">
                <div class="card h-100 bg-dark border-secondary">
                  <div class="card-body">
                    <h6 class="card-subtitle mb-2 text-info">{{ formatKey(key) }}</h6>
                    <p class="card-text text-white">{{ value }}</p>
                  </div>
                </div>
              </div>
            </div>
          </section>

          <!-- Environment Variables Section -->
          <section class="mb-5">
            <h4 class="border-bottom pb-2 mb-3">Environment Variables (ASPNETCORE / VITE)</h4>
            <div class="table-responsive">
              <table class="table table-dark table-hover table-sm">
                <thead>
                  <tr>
                    <th>Variable</th>
                    <th>Value</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(value, key) in debugInfo.environmentVariables" :key="key">
                    <td class="text-info font-monospace">{{ key }}</td>
                    <td class="font-monospace">{{ value }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import Navbar from './components/Navbar.vue';

const debugInfo = ref(null);
const loading = ref(true);
const error = ref(null);

const formatKey = (key) => {
  return key
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, (str) => str.toUpperCase())
    .trim();
};

onMounted(async () => {
  try {
    const response = await fetch('/api/debug');
    if (!response.ok) throw new Error('Failed to fetch debug info');
    debugInfo.value = await response.json();
  } catch (e) {
    error.value = e.message;
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.debug-container {
  background-color: var(--bg-darker);
  color: var(--text-primary);
}

.font-monospace {
  font-family: SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace;
  word-break: break-all;
}

.card {
  transition: transform 0.2s;
}

.card:hover {
  transform: translateY(-2px);
  border-color: var(--accent-blue) !important;
}

.x-small {
  font-size: 0.7rem;
}

.text-accent {
  color: #ff7b72;
}

h4 {
  color: var(--accent-blue);
  font-weight: 600;
}
</style>
