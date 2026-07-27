<template>
  <div class="card border-0 rounded-0 h-100 bg-transparent">
    <div class="card-body p-0 h-100">
      <div v-if="loading" class="p-4 text-center">
        <div class="spinner-border text-info mb-3" role="status"></div>
        <p class="text-muted">Loading pull requests...</p>
      </div>
      <div v-else-if="prs.length === 0" class="p-4 text-center text-light">
        <i class="bi bi-inbox fs-1 text-muted mb-3 d-block"></i>
        <p>No open pull requests found or access token not configured.</p>
      </div>
      <div v-else class="list-group list-group-flush">
        <div v-for="pr in prs" :key="pr.url" 
             @click="$emit('select-pr', pr)"
             class="list-group-item list-group-item-action bg-dark text-light border-secondary d-flex align-items-center"
             :class="{ 'active-pr': selectedPrUrl === pr.url }"
             style="cursor: pointer;">
          <div class="me-2 star-container" @click.stop="toggleStar(pr)">
            <i :class="starredUrls.has(pr.url) ? 'bi bi-star-fill text-warning' : 'bi bi-star text-muted'"></i>
          </div>
          <div class="flex-grow-1 overflow-hidden">
            <div class="d-flex w-100 justify-content-between align-items-center mb-1">
              <h6 class="mb-0 text-info text-truncate me-2 fs-base">
                {{ pr.title }}
                <span v-if="pr.isDraft" class="badge bg-secondary text-dark ms-1 fs-xxs" style="vertical-align: middle; opacity: 0.8;">DRAFT</span>
              </h6>
              <small class="fw-bold flex-shrink-0 fs-xs" :style="{ color: getStatusColor(pr.status) }">{{ pr.status }}</small>
            </div>
            <p class="mb-1 fs-sm text-light">{{ pr.repository }}</p>
            <div class="d-flex justify-content-between align-items-center">
              <small class="text-secondary fs-xs">by {{ pr.author }} on {{ formatDate(pr.createdAt) }}</small>
              <span class="badge rounded-pill bg-dark border border-secondary text-muted px-2 fs-xs" v-if="pr.number">#{{ pr.number }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { fetchGitHubPrs, fetchGitHubStarred, toggleGitHubStar } from '../js/dashboard-api';

const props = defineProps({
  selectedPrUrl: String,
  currentQuery: {
    type: String,
    default: null
  },
  showStarredOnly: {
    type: Boolean,
    default: false
  }
});

defineEmits(['select-pr']);

const prs = ref([]);
const starredUrls = ref(new Set());
const loading = ref(true);

const formatDate = (dateString) => {
  const date = new Date(dateString);
  return date.toLocaleDateString(undefined, { month: 'short', day: '2-digit', year: 'numeric' });
};

const getStatusColor = (status) => {
  const s = status.toLowerCase();
  if (s.includes('success') || s.includes('merged') || s.includes('approved')) return '#3fb950';
  if (s.includes('failure') || s.includes('error') || s.includes('rejected')) return '#f85149';
  if (s.includes('pending') || s.includes('review')) return '#f0883e';
  return '#58a6ff';
};

const loadPrs = async () => {
  loading.value = true;
  try {
    const [allPrs, starred] = await Promise.all([
      fetchGitHubPrs(props.currentQuery),
      fetchGitHubStarred()
    ]);
    
    starredUrls.value = new Set(starred);
    
    if (props.showStarredOnly) {
      prs.value = allPrs.filter(pr => starredUrls.value.has(pr.url));
    } else {
      prs.value = allPrs;
    }
  } catch (e) {
    console.error('Error fetching GitHub PRs:', e);
  } finally {
    loading.value = false;
  }
};

const toggleStar = async (pr) => {
  try {
    const result = await toggleGitHubStar(pr.url);
    if (result.isStarred) {
      starredUrls.value.add(pr.url);
    } else {
      starredUrls.value.delete(pr.url);
      if (props.showStarredOnly) {
        prs.value = prs.value.filter(p => p.url !== pr.url);
      }
    }
  } catch (e) {
    console.error('Error toggling GitHub star:', e);
  }
};

watch(() => [props.currentQuery, props.showStarredOnly], () => {
  loadPrs();
});

onMounted(() => {
  loadPrs();
});
</script>

<style scoped>
.active-pr {
  background-color: var(--bg-card) !important;
  border-left: 3px solid var(--accent-blue) !important;
}

.star-container {
  padding: 0 5px;
  cursor: pointer;
  z-index: 10;
  font-size: 1.1rem;
}

.star-container:hover .bi-star {
  color: var(--accent-yellow) !important;
}

.list-group-item {
  border-left: 3px solid transparent;
  padding: 0.75rem 1rem;
  transition: all 0.2s ease;
}

.list-group-item:hover:not(.active-pr) {
  background-color: rgba(255, 255, 255, 0.05) !important;
}

.spinner-border {
  width: 2rem;
  height: 2rem;
}
</style>
