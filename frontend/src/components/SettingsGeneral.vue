<template>
  <div class="card theme-card border-primary">
    <div class="card-body">
      <div class="row g-4">
        <div class="col-md-12">
          <label class="form-label theme-text">Theme</label>
          <div class="d-flex gap-3 mt-2">
            <div 
              class="theme-option" 
              :class="{ active: config.theme === 'Cosmic' }"
              @click="config.theme = 'Cosmic'"
            >
              <div class="theme-preview cosmic-preview"></div>
              <div class="mt-2 text-center">Cosmic</div>
            </div>
            <div 
              class="theme-option" 
              :class="{ active: config.theme === 'Light' }"
              @click="config.theme = 'Light'"
            >
              <div class="theme-preview light-preview"></div>
              <div class="mt-2 text-center">Light</div>
            </div>
          </div>
        </div>

        <div class="col-md-12">
          <hr class="theme-border my-4">
          <label class="form-label theme-text mb-1">Module Visibility</label>
          <p class="small theme-text-muted mb-3">Choose which modules are visible in the sidebar and dashboard.</p>
          <div class="row">
            <div v-for="page in allPages" :key="page.id" class="col-md-4 col-6 mb-2">
              <div class="form-check form-switch theme-text">
                <input class="form-check-input" type="checkbox" 
                       :id="'vis-' + page.id" 
                       :checked="getPageVisibility(page.id)"
                       @change="togglePageVisibility(page.id)">
                <label class="form-check-label" :for="'vis-' + page.id">{{ page.name }}</label>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  config: {
    type: Object,
    required: true
  },
  allPages: {
    type: Array,
    required: true
  }
});

const getPageVisibility = (id) => {
  if (!props.config.pageVisibilities) return true;
  const page = props.config.pageVisibilities.find(p => p.id === id);
  return page ? page.visible : true;
};

const togglePageVisibility = (id) => {
  if (!props.config.pageVisibilities) {
    props.config.pageVisibilities = [];
  }
  let page = props.config.pageVisibilities.find(p => p.id === id);
  if (!page) {
    page = { id, visible: true };
    props.config.pageVisibilities.push(page);
  }
  page.visible = !page.visible;
};
</script>

<style scoped>
.theme-option {
  cursor: pointer;
  border: 2px solid transparent;
  border-radius: 8px;
  padding: 8px;
  transition: all 0.2s;
  width: 120px;
}

.theme-option:hover {
  background-color: rgba(255, 255, 255, 0.05);
}

.theme-option.active {
  border-color: var(--accent-blue);
  background-color: rgba(88, 166, 255, 0.1);
}

.theme-preview {
  height: 60px;
  width: 100%;
  border-radius: 4px;
  border: 1px solid var(--border-primary);
}

.cosmic-preview {
  background-color: #0d1117;
  position: relative;
  overflow: hidden;
}
.cosmic-preview::after {
  content: "";
  position: absolute;
  top: 10px;
  left: 10px;
  right: 10px;
  height: 10px;
  background: #161b22;
  border-radius: 2px;
}
.cosmic-preview::before {
  content: "";
  position: absolute;
  bottom: 10px;
  left: 10px;
  width: 30px;
  height: 20px;
  background: #21262d;
  border-radius: 2px;
}

.light-preview {
  background-color: #f6f8fa;
  position: relative;
  overflow: hidden;
}
.light-preview::after {
  content: "";
  position: absolute;
  top: 10px;
  left: 10px;
  right: 10px;
  height: 10px;
  background: #ffffff;
  border: 1px solid #d0d7de;
  border-radius: 2px;
}
.light-preview::before {
  content: "";
  position: absolute;
  bottom: 10px;
  left: 10px;
  width: 30px;
  height: 20px;
  background: #ffffff;
  border: 1px solid #d0d7de;
  border-radius: 2px;
}
</style>
