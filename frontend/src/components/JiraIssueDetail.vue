<template>
  <div class="h-100 d-flex flex-column theme-bg-dark theme-text overflow-hidden">
    <div v-if="!issue" class="flex-grow-1 d-flex align-items-center justify-content-center text-muted fst-italic">
      <div class="text-center">
        <i class="bi bi-kanban display-1 mb-3 opacity-25"></i>
        <p>Select an issue to view details</p>
      </div>
    </div>
    <div v-else class="flex-grow-1 overflow-auto custom-scrollbar">
      <div class="p-4">
        <div class="mb-4 border-bottom border-secondary pb-3">
          <div class="d-flex justify-content-between align-items-start mb-2">
            <div class="flex-grow-1 overflow-hidden">
              <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0">
                  <li class="breadcrumb-item"><a :href="issue.url" target="_blank" class="text-info text-decoration-none fw-bold small text-uppercase">{{ issue.key }}</a></li>
                </ol>
              </nav>
            </div>
            <div class="d-flex gap-2 ms-3 flex-shrink-0 align-items-center">
              <span v-if="issue.issueType" class="detail-badge opacity-75">{{ issue.issueType }}</span>
              <span class="detail-badge" :style="{ color: getStatusColor(issue.status) }">{{ issue.status }}</span>
              <span class="detail-badge" :style="{ color: getPriorityColor(issue.priority) }">{{ issue.priority }}</span>
            </div>
          </div>
          <h2 class="h3 mb-0 theme-text w-100">{{ issue.summary }}</h2>
        </div>

      <div class="row mb-4">
        <div class="col-md-4">
          <div class="detail-label fs-xxs text-uppercase fw-bold mb-1 opacity-75">Reporter</div>
          <div class="d-flex align-items-center">
            <i class="bi bi-person-badge me-2 fs-5 text-muted"></i>
            <span>{{ issue.reporter || 'Unknown' }}</span>
          </div>
        </div>
        <div class="col-md-4">
          <div class="detail-label fs-xxs text-uppercase fw-bold mb-1 opacity-75">Assignee</div>
          <div class="d-flex align-items-center">
            <i class="bi bi-person-circle me-2 fs-5 text-muted"></i>
            <span>{{ issue.assignee || 'Unassigned' }}</span>
          </div>
        </div>
        <div class="col-md-4">
          <div class="detail-label fs-xxs text-uppercase fw-bold mb-1 opacity-75">Status</div>
          <div class="d-flex align-items-center">
             <i class="bi bi-circle-fill me-2" :style="{ color: getStatusColor(issue.status) }"></i>
             <span>{{ issue.status }}</span>
          </div>
        </div>
      </div>

      <div class="mb-4">
        <div class="detail-label fs-xxs text-uppercase fw-bold mb-2 opacity-75">Description</div>
        <div class="description-content p-3 rounded w-100" style="background-color: var(--bg-card); border: 1px solid var(--border-primary);">
          <div v-if="issue.description" v-html="formatDescription(issue.description)" class="jira-description w-100"></div>
          <div v-else class="text-muted fst-italic">No description provided.</div>
        </div>
      </div>

      <div class="mb-4">
        <div class="detail-label fs-xxs text-uppercase fw-bold mb-3 opacity-75">Comments</div>
        <div v-if="issue.comments && issue.comments.length > 0" class="comments-list">
          <div v-for="(comment, index) in issue.comments" :key="comment.id || index" class="comment-item mb-3 p-3 rounded w-100" style="background-color: var(--bg-darker); border: 1px solid var(--border-primary);">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <span class="fw-bold text-info">{{ comment.author }}</span>
              <div class="d-flex align-items-center gap-2">
                <span class="small text-muted fst-italic fs-xs">{{ formatDate(comment.created) }}</span>
                <button 
                  v-if="canDelete(comment)" 
                  @click="deleteComment(comment)" 
                  class="btn btn-link p-0 text-danger opacity-50 hover-opacity-100" 
                  title="Delete comment"
                  :disabled="deletingId === comment.id"
                >
                  <span v-if="deletingId === comment.id" class="spinner-border spinner-border-sm" role="status"></span>
                  <i v-else class="bi bi-trash fs-xs"></i>
                </button>
              </div>
            </div>
            <div v-html="formatDescription(comment.body)" class="jira-comment-body w-100"></div>
          </div>
        </div>
        <div v-else class="p-3 rounded text-muted fst-italic" style="background-color: var(--bg-card); border: 1px solid var(--border-primary);">
          No comments yet.
        </div>
      </div>

      <div v-if="issue.comments !== undefined" class="mb-4">
        <div class="detail-label fs-xxs text-uppercase fw-bold mb-3 opacity-75">Add Comment</div>
        <div class="p-3 rounded add-comment-container" style="background-color: var(--bg-card); border: 1px solid var(--border-primary);">
          <textarea 
            v-model="newComment" 
            class="form-control theme-input mb-2" 
            rows="3" 
            placeholder="Type your comment here..."
            :disabled="isSubmitting"
          ></textarea>
          <div class="d-flex justify-content-end">
            <button 
              @click="submitComment" 
              class="btn btn-sm btn-primary px-3" 
              :disabled="!newComment.trim() || isSubmitting"
            >
              <span v-if="isSubmitting" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
              <i v-else class="bi bi-chat-left-text me-1"></i>
              Add Comment
            </button>
          </div>
          <div v-if="submitError" class="text-danger small mt-2">
            {{ submitError }}
          </div>
        </div>
      </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';

const props = defineProps({
  issue: {
    type: Object,
    default: null
  }
});

const newComment = ref('');
const isSubmitting = ref(false);
const submitError = ref(null);
const deletingId = ref(null);
const currentUser = ref(null);

onMounted(async () => {
  try {
    const response = await fetch('/api/jira/myself');
    if (response.ok) {
      currentUser.value = await response.json();
    }
  } catch (e) {
    console.error('Failed to fetch current user', e);
  }
});

const canDelete = (comment) => {
  if (!comment || !comment.id) return false;
  
  const author = comment.author?.toLowerCase() || '';
  
  // Demo Mode check
  if (author === 'you' || author.includes('(demo)')) return true;
  
  // Check against current user from API
  if (currentUser.value) {
    const currentName = currentUser.value.displayName?.toLowerCase() || '';
    if (author === currentName) return true;
  }
  
  return false;
};

const deleteComment = async (comment) => {
  if (!comment || !comment.id || !props.issue) return;
  if (!confirm('Are you sure you want to delete this comment?')) return;
  
  deletingId.value = comment.id;
  
  try {
    const response = await fetch(`/api/jira/comment?key=${props.issue.key}&id=${comment.id}`, {
      method: 'DELETE'
    });
    
    if (response.ok) {
      // Remove from local list
      props.issue.comments = props.issue.comments.filter(c => c.id !== comment.id);
    } else {
      const errorText = await response.text();
      alert(`Failed to delete comment: ${errorText || response.statusText}`);
    }
  } catch (e) {
    alert(`Error: ${e.message}`);
  } finally {
    deletingId.value = null;
  }
};

const submitComment = async () => {
  if (!newComment.value.trim() || !props.issue) return;
  
  isSubmitting.value = true;
  submitError.value = null;
  
  try {
    const response = await fetch(`/api/jira/comment?key=${props.issue.key}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ body: newComment.value })
    });
    
    if (response.ok) {
      const createdComment = await response.json();
      
      // Add the comment locally for immediate feedback
      if (!props.issue.comments) {
        props.issue.comments = [];
      }
      
      props.issue.comments.push(createdComment);
      
      newComment.value = '';
    } else {
      const errorText = await response.text();
      submitError.value = `Failed to add comment: ${errorText || response.statusText}`;
    }
  } catch (e) {
    submitError.value = `Error: ${e.message}`;
  } finally {
    isSubmitting.value = false;
  }
};

const getStatusColor = (status) => {
  if (!status) return '#aab2bb';
  const s = status.toLowerCase();
  if (s.includes('done') || s.includes('closed') || s.includes('resolved')) return '#3fb950';
  if (s.includes('progress')) return '#58a6ff';
  if (s.includes('todo') || s.includes('backlog')) return '#aab2bb';
  return '#bc8cff';
};

const getPriorityColor = (priority) => {
  if (!priority) return '#aab2bb';
  const p = priority.toLowerCase();
  if (p.includes('highest') || p.includes('critical')) return '#f85149';
  if (p.includes('high')) return '#f0883e';
  if (p.includes('medium')) return '#ffa500';
  if (p.includes('low')) return '#3fb950';
  return '#aab2bb';
};

const formatDescription = (description) => {
  if (!description) return '';
  
  // If it's a string, it might be raw JSON (ADF) from the API or a simple string
  if (typeof description === 'string') {
    if (description.startsWith('{')) {
      try {
        const parsed = JSON.parse(description);
        return formatADF(parsed);
      } catch (e) {
        // Not valid JSON after all, treat as plain text
      }
    }
    return description.replace(/\n/g, '<br>');
  }
  
  return formatADF(description);
};

const formatADF = (adf) => {
  if (!adf) return '';
  
  // Very basic ADF to HTML converter
  // Handles: doc, paragraph, text, bulletList, listItem
  
  if (adf.type === 'doc') {
    return adf.content ? adf.content.map(formatADF).join('') : '';
  }
  
  if (adf.type === 'paragraph') {
    return `<p>${adf.content ? adf.content.map(formatADF).join('') : ''}</p>`;
  }
  
  if (adf.type === 'text') {
    let text = adf.text || '';
    if (adf.marks) {
      adf.marks.forEach(mark => {
        if (mark.type === 'strong') text = `<strong>${text}</strong>`;
        if (mark.type === 'em') text = `<em>${text}</em>`;
        if (mark.type === 'underline') text = `<u>${text}</u>`;
        if (mark.type === 'link') text = `<a href="${mark.attrs.href}" target="_blank">${text}</a>`;
      });
    }
    return text;
  }
  
  if (adf.type === 'bulletList') {
    return `<ul>${adf.content ? adf.content.map(formatADF).join('') : ''}</ul>`;
  }
  
  if (adf.type === 'orderedList') {
    return `<ol>${adf.content ? adf.content.map(formatADF).join('') : ''}</ol>`;
  }
  
  if (adf.type === 'listItem') {
    return `<li>${adf.content ? adf.content.map(formatADF).join('') : ''}</li>`;
  }

  if (adf.type === 'heading') {
    const level = adf.attrs?.level || 1;
    // Map heading levels to smaller ones: 1->3, 2->4, 3->5, others->6
    const newLevel = Math.min(level + 2, 6);
    return `<h${newLevel}>${adf.content ? adf.content.map(formatADF).join('') : ''}</h${newLevel}>`;
  }

  if (adf.type === 'codeBlock') {
     return `<pre><code>${adf.content ? adf.content.map(formatADF).join('') : ''}</code></pre>`;
  }

  if (adf.type === 'hardBreak') {
    return '<br>';
  }
  
  return '';
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

<style>
.detail-label {
  letter-spacing: 0.05em !important;
  color: #aab2bb !important;
}

.jira-description, .jira-comment-body {
  line-height: 1.6 !important;
  word-break: break-word !important;
  color: #c9d1d9 !important;
  overflow-wrap: anywhere !important;
  width: 100% !important;
}

.jira-description pre, .jira-comment-body pre {
  white-space: pre-wrap !important;
  word-break: break-all !important;
  background-color: var(--bg-darker);
  padding: 10px;
  border-radius: 4px;
}

.jira-description h1, .jira-description h2, .jira-description h3, .jira-description h4, .jira-description h5, .jira-description h6,
.jira-comment-body h1, .jira-comment-body h2, .jira-comment-body h3, .jira-comment-body h4, .jira-comment-body h5, .jira-comment-body h6 {
  margin-top: 1rem !important;
  margin-bottom: 0.5rem !important;
  font-weight: 600 !important;
  line-height: 1.2 !important;
  color: #c9d1d9 !important;
}

.jira-description h3, .jira-comment-body h3 { font-size: 1.25rem !important; }
.jira-description h4, .jira-comment-body h4 { font-size: 1.1rem !important; }
.jira-description h5, .jira-comment-body h5 { font-size: 1rem !important; }
.jira-description p, .jira-comment-body p {
  width: 100% !important;
  max-width: 100% !important;
  margin-bottom: 1rem !important;
  overflow-wrap: anywhere !important;
}

.breadcrumb-item + .breadcrumb-item::before {
    color: #8b949e !important;
}

.detail-badge {
  font-size: 0.65rem;
  line-height: 1;
  padding: 2px 6px;
  border: 1px solid currentColor;
  border-radius: 4px;
  background-color: rgba(255, 255, 255, 0.05);
  white-space: nowrap;
  display: inline-block;
  font-weight: 600;
}

.add-comment-container:focus-within {
  border-color: var(--accent-blue) !important;
  box-shadow: 0 0 0 1px var(--accent-blue);
}

.hover-opacity-100:hover {
  opacity: 1 !important;
}

.fs-xs {
  font-size: 0.75rem !important;
}
</style>
