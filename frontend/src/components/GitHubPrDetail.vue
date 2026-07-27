<template>
  <div class="h-100 d-flex flex-column bg-dark text-light overflow-hidden">
    <div v-if="!pr" class="flex-grow-1 d-flex align-items-center justify-content-center" style="color: #ffffff !important; font-style: italic;">
      <div class="text-center">
        <i class="bi bi-github display-1 mb-3"></i>
        <p>Select a pull request to view details</p>
      </div>
    </div>
    <div v-else class="flex-grow-1 overflow-auto p-4">
      <div class="d-flex justify-content-between align-items-start mb-4">
        <div>
          <nav aria-label="breadcrumb">
            <ol class="breadcrumb mb-2">
              <li class="breadcrumb-item">
                <a :href="pr.url" target="_blank" class="text-info text-decoration-none fw-bold">
                  {{ pr.repository }} #{{ pr.number }}
                </a>
              </li>
            </ol>
          </nav>
          <h2 class="h3 mb-0">{{ pr.title }}</h2>
        </div>
        <div class="d-flex gap-2">
          <span v-if="pr.isDraft" class="badge bg-secondary text-dark border border-secondary">DRAFT</span>
          <span class="badge" :style="{ backgroundColor: getStatusColor(pr.status) + ' !important' }">{{ pr.status }}</span>
        </div>
      </div>

      <div class="row mb-4">
        <div class="col-md-6">
          <div class="detail-label small text-uppercase fw-bold mb-1" style="color: #ffffff !important;">Author</div>
          <div class="d-flex align-items-center">
            <i class="bi bi-person-circle me-2 fs-5"></i>
            <span>{{ pr.author || 'Unknown' }}</span>
          </div>
        </div>
        <div class="col-md-6">
          <div class="detail-label small text-uppercase fw-bold mb-1" style="color: #ffffff !important;">Created</div>
          <div class="d-flex align-items-center">
            <i class="bi bi-calendar3 me-2"></i>
            <span>{{ formatDate(pr.createdAt) }}</span>
          </div>
        </div>
      </div>

      <div class="mb-4">
        <div class="detail-label small text-uppercase fw-bold mb-2" style="color: #ffffff !important;">Description</div>
        <div class="description-content p-3 rounded" style="background-color: #21262d; border: 1px solid #30363d;">
          <div v-if="pr.body" class="github-body" v-html="formatBody(pr.body)"></div>
          <div v-else style="color: #ffffff !important; font-style: italic; display: block; min-height: 1.5em;">No description provided.</div>
        </div>
      </div>

      <div class="mb-4">
        <div class="detail-label small text-uppercase fw-bold mb-3" style="color: #ffffff !important; font-style: normal;">Conversation</div>
        <div v-if="pr.comments && pr.comments.length > 0" class="comments-list">
          <div v-for="(comment, index) in pr.comments" :key="index" class="comment-item mb-3 p-3 rounded" style="background-color: var(--bg-darker); border: 1px solid var(--border-primary);">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <span class="fw-bold text-info">{{ comment.author }}</span>
              <span class="small" style="color: #ffffff !important; font-style: italic;">{{ formatDate(comment.createdAt) }}</span>
            </div>
            <div v-html="formatBody(comment.body)" class="github-comment-body"></div>
          </div>
        </div>
        <div v-else class="p-3 rounded" style="background-color: #21262d; border: 1px solid #30363d; color: #ffffff !important; font-style: italic; display: block; min-height: 1.5em;">
          No comments yet.
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  pr: {
    type: Object,
    default: null
  }
});

const getStatusColor = (status) => {
  if (!status) return '#aab2bb';
  const s = status.toLowerCase();
  if (s.includes('success') || s.includes('merged') || s.includes('approved') || s.includes('open')) return '#3fb950';
  if (s.includes('failure') || s.includes('error') || s.includes('rejected')) return '#f85149';
  if (s.includes('pending') || s.includes('review')) return '#f0883e';
  return '#58a6ff';
};

const formatBody = (body) => {
  if (!body) return '';
  // Basic markdown-like newline to br conversion
  return body
    .replace(/\r\n/g, '<br>')
    .replace(/\n/g, '<br>')
    .replace(/### (.*?)<br>/g, '<h3>$1</h3>')
    .replace(/## (.*?)<br>/g, '<h2>$1</h2>')
    .replace(/# (.*?)<br>/g, '<h1>$1</h1>')
    .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
    .replace(/\*(.*?)\*/g, '<em>$1</em>')
    .replace(/`(.*?)`/g, '<code>$1</code>');
};

const formatDate = (dateStr) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return date.toLocaleString(undefined, { 
    year: 'numeric', 
    month: 'short', 
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};
</script>

<style scoped>
.detail-label {
  letter-spacing: 0.05em;
}

.github-body, .github-comment-body {
  line-height: 1.6;
  word-break: break-word;
}

.github-body h1, .github-body h2, .github-body h3, .github-body h4, .github-body h5, .github-body h6,
.github-comment-body h1, .github-comment-body h2, .github-comment-body h3, .github-comment-body h4, .github-comment-body h5, .github-comment-body h6 {
  margin-top: 1rem;
  margin-bottom: 0.5rem;
  font-weight: 600;
  line-height: 1.2;
}

.github-body h3, .github-comment-body h3 { font-size: 1.25rem; }
.github-body h2, .github-comment-body h2 { font-size: 1.5rem; }
.github-body h1, .github-comment-body h1 { font-size: 1.75rem; }

.breadcrumb-item + .breadcrumb-item::before {
    color: var(--text-muted);
}
</style>
