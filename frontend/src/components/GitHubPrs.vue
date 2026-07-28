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
      <div v-else-if="filteredPrs.length === 0" class="p-5 text-center text-muted">
        <i class="bi bi-search display-4 mb-3 opacity-25"></i>
        <p>No pull requests match your search "{{ searchQuery }}".</p>
      </div>
      <div v-else class="github-prs-list">
        <div class="github-list-header">
          <div class="col-title">Title</div>
          <div class="col-repo">Repository</div>
          <div class="col-status">Status</div>
          <div class="col-author">Author</div>
          <div class="col-number">#</div>
        </div>
        <div v-for="pr in filteredPrs" :key="pr.url" 
             @click="$emit('select-pr', pr)"
             class="github-pr-row"
             :class="{ selected: selectedPrUrl === pr.url }"
             style="cursor: pointer;">
          <div class="github-pr-main-row">
            <div class="col-title">
              <div class="d-flex align-items-center">
                <div class="star-container me-2" @click.stop="toggleStar(pr)">
                  <i :class="starredUrls.has(pr.url) ? 'bi bi-star-fill text-warning' : 'bi bi-star text-muted'"></i>
                </div>
                <span class="text-info fw-bold truncate-text">
                  {{ pr.title }}
                  <span v-if="pr.isDraft" class="draft-badge ms-1">DRAFT</span>
                </span>
              </div>
            </div>
            <div class="col-repo">
              <span class="text-light truncate-text">{{ pr.repository }}</span>
            </div>
            <div class="col-status">
              <span class="status-text fw-bold" :style="{ color: getStatusColor(pr.status) }">{{ pr.status }}</span>
            </div>
            <div class="col-author">
              <span class="text-secondary truncate-text"><i class="bi bi-person me-1"></i> {{ pr.author }}</span>
            </div>
            <div class="col-number">
              <span class="badge rounded-pill bg-dark border border-secondary text-muted px-2 fs-xs">#{{ pr.number }}</span>
            </div>
          </div>
          <div class="github-pr-sub-row">
            <div class="col-spacer"></div>
            <div class="col-description">
              <span v-if="pr.body" class="text-muted fs-xs truncate-text">{{ pr.body }}</span>
              <span v-else class="text-muted fs-xs italic">No description</span>
            </div>
            <div class="col-dates text-muted fs-xxs">
              <span>Opened {{ formatFriendlyDate(pr.createdAt, false, true) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { fetchGitHubPrs, fetchGitHubStarred, toggleGitHubStar } from '../js/dashboard-api';
import { formatFriendlyDate } from '../js/utils';

const props = defineProps({
  selectedPrUrl: String,
  currentQuery: {
    type: String,
    default: null
  },
  searchQuery: {
    type: String,
    default: ''
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

const filteredPrs = computed(() => {
  if (!props.searchQuery) return prs.value;
  
  const query = props.searchQuery.toLowerCase();
  return prs.value.filter(pr => {
    return (
      (pr.title && pr.title.toLowerCase().includes(query)) ||
      (pr.author && pr.author.toLowerCase().includes(query)) ||
      (pr.repository && pr.repository.toLowerCase().includes(query)) ||
      (pr.status && pr.status.toLowerCase().includes(query)) ||
      (pr.number && pr.number.toString().includes(query))
    );
  });
});

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
.github-prs-list {
  display: flex;
  flex-direction: column;
  height: 100%;
  background-color: var(--bg-dark);
}

.github-list-header {
  display: flex;
  align-items: center;
  padding: 8px 12px;
  background-color: var(--bg-dark);
  border-bottom: 1px solid var(--border-primary);
  font-size: var(--fs-xxs);
  text-transform: uppercase;
  font-weight: 700;
  color: var(--text-muted);
  position: sticky;
  top: 0;
  z-index: 10;
}

.github-pr-row {
  display: flex;
  flex-direction: column;
  border-bottom: 1px solid var(--border-primary);
  cursor: pointer;
  transition: background-color 0.2s;
  font-size: var(--fs-sm);
  min-height: 64px;
}

.github-pr-row:hover {
  background-color: rgba(255, 255, 255, 0.03);
}

.github-pr-row.selected {
  background-color: rgba(88, 166, 255, 0.1);
  border-left: 3px solid var(--accent-blue) !important;
}

.github-pr-main-row, .github-pr-sub-row {
  display: flex;
  align-items: center;
  width: 100%;
  padding: 8px 12px;
}

.github-pr-main-row {
  padding-bottom: 4px;
}

.github-pr-sub-row {
  padding-top: 0;
  margin-top: -4px;
}

/* Column Widths */
.col-title { flex-grow: 1; min-width: 200px; padding-right: 12px; overflow: hidden; }
.col-repo { width: 150px; flex-shrink: 0; padding-right: 12px; overflow: hidden; }
.col-status { width: 100px; flex-shrink: 0; padding-right: 8px; }
.col-author { width: 120px; flex-shrink: 0; padding-right: 8px; }
.col-number { width: 60px; flex-shrink: 0; text-align: right; }

.col-spacer {
  width: 40px; 
  flex-shrink: 0;
}

.col-description {
  flex-grow: 1;
  min-width: 200px;
  padding-right: 12px;
  overflow: hidden;
}

.col-dates {
  width: 200px;
  flex-shrink: 0;
  text-align: right;
}

.truncate-text {
  display: block;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.draft-badge {
  font-size: 0.65rem;
  line-height: 1;
  padding: 2px 4px;
  background-color: #2c313a;
  color: var(--text-primary);
  border-radius: 3px;
  vertical-align: middle;
  opacity: 0.8;
}

.star-container {
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  font-size: 1.1rem;
}

.star-container:hover .bi-star {
  color: var(--accent-yellow) !important;
}

.spinner-border {
  width: 2rem;
  height: 2rem;
}
</style>
