<template>
  <div class="card theme-card">
    <div class="card-body">
      <p class="small theme-text-muted mb-3">Choose which modules are visible in the sidebar and dashboard.</p>
      <div class="row">
        <div v-for="page in allPages" :key="page.id" class="col-6 mb-2">
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
</template>

<script setup>
const props = defineProps({
  pageVisibilities: {
    type: Array,
    required: true
  },
  allPages: {
    type: Array,
    required: true
  }
});

const getPageVisibility = (id) => {
  const page = props.pageVisibilities.find(p => p.id === id);
  return page ? page.visible : true;
};

const togglePageVisibility = (id) => {
  let page = props.pageVisibilities.find(p => p.id === id);
  if (!page) {
    page = { id, visible: true };
    props.pageVisibilities.push(page);
  }
  page.visible = !page.visible;
};
</script>
