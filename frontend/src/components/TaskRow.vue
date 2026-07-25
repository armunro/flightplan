<template>
    <div class="task-row-container" :class="{ 'is-selected': isSelected }">
        <div class="tasks-row" 
             draggable="true" 
             @dragstart="onDragStart"
             @dragover="onDragOver"
             @dragleave="onDragLeave"
             @drop="onDrop"
             @contextmenu.prevent="onContextMenu"
             @dblclick="onOpenTask"
             :class="[dropClass, { 'has-subtasks': task && task.subtasks && task.subtasks.length > 0 }]"
             :style="gridStyle">
            <div class="tasks-cell selection-cell">
                <input type="checkbox" :checked="isSelected" @mousedown="onCheckboxMouseDown($event)" @click.stop="onCheckboxClick($event)">
            </div>
            <div class="tasks-cell task-title-container" 
                 :style="depth > 0 ? { paddingLeft: (depth * 20) + 24 + 'px' } : {}">
                <span v-if="depth > 0" class="subtask-indent"></span>
                <span class="task-title"
                      ref="titleElement"
                      contenteditable="true" 
                      tabindex="0"
                      @blur="onUpdateTitle"
                      @keydown.enter="onTitleKeyDown"
                      @keydown.tab="onTitleTabKeyDown"
                      @keydown.esc="onTitleEsc"
                      @keydown="onGeneralKeyDown"
                      @paste="onPaste">{{ task?.title || '' }}</span>
                <a v-if="task?.link" :href="task.link" target="_blank" class="action-link-icon" title="Open link"></a>
            </div>
            <div class="tasks-cell" :style="{ width: gridStyle.gridTemplateColumns.split(' ')[2] }">
                <div class="dropdown" v-if="task" :class="{ 'dropup': isLast }">
                    <span class="type-badge dropdown-toggle" 
                          data-bs-toggle="dropdown"
                          data-bs-auto-close="outside"
                          :style="{ color: getTaskTypeColor(task.taskTypeId) }">
                        <i :class="getTaskTypeIcon(task.taskTypeId)"></i>
                        <span class="ms-1">{{ getTaskTypeName(task.taskTypeId) }}</span>
                    </span>
                    <ul class="dropdown-menu dropdown-menu-dark">
                        <li><a class="dropdown-item" href="#" @click.prevent="onUpdateTaskType({ target: { value: null } })">-- Type --</a></li>
                        <li v-for="t in projectTaskTypes" :key="t.id">
                            <a class="dropdown-item" href="#" @click.prevent="onUpdateTaskType({ target: { value: t.id } })" :style="{ color: t.color }">
                                <i :class="t.icon" class="me-2"></i>{{ t.name }}
                            </a>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="tasks-cell" :style="{ width: gridStyle.gridTemplateColumns.split(' ')[3] }">
                <div class="dropdown" v-if="task" :class="{ 'dropup': isLast }">
                    <span class="status-badge dropdown-toggle" 
                          data-bs-toggle="dropdown"
                          data-bs-auto-close="outside"
                          :style="{ color: getStatusColor(task.statusId) }">
                        <i class="bi bi-circle-fill" style="font-size: 8px; margin-right: 4px;"></i>
                        <span>{{ getStatusName(task.statusId) }}</span>
                    </span>
                    <ul class="dropdown-menu dropdown-menu-dark">
                        <li v-for="s in projectStatuses" :key="s.id">
                            <a class="dropdown-item" href="#" @click.prevent="onUpdateStatus({ target: { value: s.id } })" :style="{ color: s.color }">
                                <i class="bi bi-circle-fill me-2" style="font-size: 8px;"></i>{{ s.name }}
                            </a>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="tasks-cell" :style="{ width: gridStyle.gridTemplateColumns.split(' ')[4] }">
                <div class="dropdown" v-if="task" :class="{ 'dropup': isLast }">
                    <span class="priority priority-badge dropdown-toggle" 
                          data-bs-toggle="dropdown"
                          data-bs-auto-close="outside"
                          :style="{ color: getPriorityColor(task.priorityId) }">
                        <i :class="getPriorityIcon(task.priorityId)" style="margin-right: 4px;"></i>
                        <span>{{ getPriorityName(task.priorityId) }}</span>
                    </span>
                    <ul class="dropdown-menu dropdown-menu-dark">
                        <li v-for="p in projectPriorities" :key="p.id">
                            <a class="dropdown-item" href="#" @click.prevent="onUpdatePriorityId(p.id)" :style="{ color: p.color }">
                                <i :class="p.icon" class="me-2"></i>{{ p.name }}
                            </a>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="tasks-cell date-cell" :style="{ width: gridStyle.gridTemplateColumns.split(' ')[5] }">
                <date-time-selector 
                    :model-value="task?.start" 
                    placeholder="Start"
                    :is-closed="task?.isCompleted"
                    @update:model-value="onUpdateDate('start', $event)"
                    size="small"
                />
            </div>
            <div class="tasks-cell date-cell" :style="{ width: gridStyle.gridTemplateColumns.split(' ')[6] }">
                <date-time-selector 
                    :model-value="task?.end" 
                    placeholder="End"
                    :is-closed="task?.isCompleted"
                    @update:model-value="onUpdateDate('end', $event)"
                    size="small"
                />
            </div>
            <div class="tasks-cell estimate-cell clickable" :style="{ width: gridStyle.gridTemplateColumns.split(' ')[7] }" @click="startEditingEstimate">
                <template v-if="!isEditingEstimate">
                    {{ formatEstimate(task?.estimateMinutes) }}
                </template>
                <input v-else
                       type="text"
                       class="estimate-input"
                       v-focus
                       :value="task?.estimateMinutes"
                       @blur="onUpdateEstimate"
                       @keyup.enter="$event.target.blur()">
            </div>
        </div>
        <template v-if="task && task.subtasks">
            <template v-for="(sub, index) in getSortedSubtasks(task.subtasks)" :key="sub.id">
                <task-row :task="sub" 
                          :depth="depth + 1" 
                          :parent-task-id="task.id"
                          :previous-task-id="index > 0 ? getSortedSubtasks(task.subtasks)[index-1].id : null"
                          :project-statuses="projectStatuses" 
                          :project-task-types="projectTaskTypes" 
                          :project-priorities="projectPriorities" 
                          :showClosed="showClosed" 
                          :grid-style="gridStyle" 
                          :is-last="isLast && index === getSortedSubtasks(task.subtasks).length - 1"
                          :selected-task-ids="selectedTaskIds"
                          @refresh="$emit('refresh')" 
                          @open-task="$emit('open-task', $event)"
                          @toggle-select="$emit('toggle-select', $event)"
                          @context-menu="(e, task) => $emit('context-menu', e, task)"></task-row>
            </template>
        </template>
    </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue';
import DateTimeSelector from './DateTimeSelector.vue';
import { updateTask, addSubtask, addSibling, deleteTask, moveTask } from '../js/tasks-api';
import { formatFriendlyDate, formatForInput, formatToISO, formatEstimate, parseEstimate } from '../js/utils';

export default {
    name: 'TaskRow',
    components: {
        DateTimeSelector
    },
    props: ['task', 'depth', 'projectStatuses', 'projectTaskTypes', 'projectPriorities', 'showClosed', 'gridStyle', 'isLast', 'selectedTaskIds', 'parentTaskId', 'previousTaskId'],
    emits: ['refresh', 'open-task', 'context-menu', 'toggle-select'],
    setup(props, { emit }) {
        const dropPosition = ref(null); // 'before', 'after', 'inside'
        const isEditingEstimate = ref(false);
        const titleElement = ref(null);

        const isSelected = computed(() => props.selectedTaskIds?.includes(props.task.id));

        const getSortedSubtasks = (subtasks) => {
            if (!subtasks) return [];
            return subtasks.filter(s => props.showClosed || !s.isCompleted);
        };

        onMounted(() => {
            if (props.task && !props.task.title && titleElement.value) {
                titleElement.value.focus();
            }
        });

        const getPriorityName = (priorityId) => {
            const priority = props.projectPriorities?.find(p => p.id === priorityId);
            return priority ? priority.name : '';
        };

        const getStatusName = (statusId) => {
            const status = props.projectStatuses?.find(s => s.id === statusId);
            return status ? status.name : 'Unknown';
        };

        const getStatusColor = (statusId) => {
            const status = props.projectStatuses?.find(s => s.id === statusId);
            return status ? status.color : '#cccccc';
        };

        const getTaskTypeName = (typeId) => {
            const type = props.projectTaskTypes?.find(t => t.id === typeId);
            return type ? type.name : 'Work';
        };

        const getTaskTypeColor = (typeId) => {
            const type = props.projectTaskTypes?.find(t => t.id === typeId);
            return type ? type.color : '#3498db';
        };

        const getTaskTypeIcon = (typeId) => {
            const type = props.projectTaskTypes?.find(t => t.id === typeId);
            return type ? type.icon : 'bi-briefcase';
        };

        const getPriorityIcon = (priorityId) => {
            const priority = props.projectPriorities?.find(p => p.id === priorityId);
            return priority ? priority.icon : 'bi-dash-lg';
        };

        const getPriorityColor = (priorityId) => {
            const priority = props.projectPriorities?.find(p => p.id === priorityId);
            return priority ? priority.color : '#ccc';
        };

        const onDragStart = (e) => {
            console.log('[DEBUG_LOG] Task drag start:', props.task.id);
            // Use unique MIME types to avoid ambiguity during dragover
            const taskMimeType = 'application/x-flightplan-task';
            e.dataTransfer.setData(taskMimeType, props.task.id);
            e.dataTransfer.setData('text/plain', props.task.id);
            e.dataTransfer.setData('taskId', props.task.id); // Keeping for backward compatibility if needed
            e.dataTransfer.effectAllowed = 'move';
            e.stopPropagation();
        };

        const onCheckboxMouseDown = (e) => {
            // Store shift key on mousedown as well, just in case
            e.target._lastShiftKey = e.shiftKey;
        };

        const onCheckboxClick = (e) => {
            const shiftKey = e.shiftKey || e.target._lastShiftKey;
            emit('toggle-select', { taskId: props.task.id, shiftKey: !!shiftKey });
        };

        const onDragOver = (e) => {
            const types = Array.from(e.dataTransfer.types);
            const isDraggingTask = types.some(t => t.toLowerCase() === 'application/x-flightplan-task' || t.toLowerCase() === 'taskid');
            
            if (!isDraggingTask) return;

            e.preventDefault();
            e.stopPropagation();
            
            const rect = e.currentTarget.getBoundingClientRect();
            const y = e.clientY - rect.top;
            const threshold = rect.height / 3;

            if (y < threshold) {
                dropPosition.value = 'before';
            } else if (y > rect.height - threshold) {
                dropPosition.value = 'after';
            } else {
                dropPosition.value = 'inside';
            }
        };

        const onDragLeave = (e) => {
            dropPosition.value = null;
        };

        const onDrop = async (e) => {
            const types = Array.from(e.dataTransfer.types);
            const isDraggingTask = types.some(t => t.toLowerCase() === 'application/x-flightplan-task' || t.toLowerCase() === 'taskid');
            
            if (!isDraggingTask) return;

            e.preventDefault();
            e.stopPropagation();
            const pos = dropPosition.value;
            dropPosition.value = null;

            console.log('[DEBUG_LOG] Task drop target:', props.task.id);
            let draggedTaskId = e.dataTransfer.getData('application/x-flightplan-task');
            if (!draggedTaskId) {
                draggedTaskId = e.dataTransfer.getData('taskId');
            }
            if (!draggedTaskId) {
                draggedTaskId = e.dataTransfer.getData('text/plain');
            }
            console.log('[DEBUG_LOG] Dragged task ID:', draggedTaskId);

            if (draggedTaskId && draggedTaskId !== props.task.id) {
                const positionMap = { 'before': 'Before', 'after': 'After', 'inside': 'Inside' };
                await moveTask(draggedTaskId, null, props.task.id, positionMap[pos]);
                emit('refresh');
            }
        };

        const dropClass = computed(() => {
            if (!dropPosition.value) return '';
            return `drag-over-${dropPosition.value}`;
        });


        const onTitleKeyDown = async (e) => {
            console.log('[DEBUG_LOG] onTitleKeyDown triggered', e.key);
            e.preventDefault();
            e.stopPropagation();
            // If Enter is pressed, create a new sibling task
            const defaultStatusId = props.projectStatuses?.length > 0 ? props.projectStatuses[0].id : null;
            console.log('[DEBUG_LOG] Adding sibling for task:', props.task.id);
            try {
                // Ensure the current title is saved before creating sibling
                await onUpdateTitle(e);
                const newSibling = await addSibling(props.task.id, "", defaultStatusId); 
                console.log('[DEBUG_LOG] Sibling added:', newSibling);
                emit('refresh');
            } catch (err) {
                console.error('[DEBUG_LOG] Error adding sibling:', err);
            }
        };

        const onTitleTabKeyDown = async (e) => {
            console.log('[DEBUG_LOG] onTitleTabKeyDown triggered', e.key, 'Shift:', e.shiftKey);
            e.preventDefault();
            e.stopPropagation();

            try {
                // Ensure the current title is saved before moving
                await onUpdateTitle(e);

                if (e.shiftKey) {
                    // Shift+Tab: Promote task (outdent)
                    if (props.parentTaskId) {
                        console.log('[DEBUG_LOG] Promoting task:', props.task.id, 'to be sibling of:', props.parentTaskId);
                        await moveTask(props.task.id, null, props.parentTaskId, 'After'); // Shift+Tab: Promote task (outdent)
                        emit('refresh');
                    }
                } else {
                    // Tab: Demote task (indent)
                    if (props.previousTaskId) {
                        console.log('[DEBUG_LOG] Demoting task:', props.task.id, 'to be subtask of:', props.previousTaskId);
                        await moveTask(props.task.id, null, props.previousTaskId, 'Inside'); // Tab: Demote task (indent)
                        emit('refresh');
                    }
                }
            } catch (err) {
                console.error('[DEBUG_LOG] Error moving task:', err);
            }
        };

        const onTitleEsc = (e) => {
            console.log('[DEBUG_LOG] onTitleEsc triggered');
            e.target.innerText = props.task.title;
            e.target.blur();
        };

        const onGeneralKeyDown = (e) => {
            console.log('[DEBUG_LOG] onGeneralKeyDown:', e.key, 'target:', e.target.tagName, 'content:', JSON.stringify(e.target.textContent));
            if (e.key === 'Tab') {
                e.preventDefault();
                e.stopPropagation();
                onTitleTabKeyDown(e);
            } else if (e.key === 'Delete' || e.key === 'Backspace') {
                const text = e.target.textContent || '';
                // Use a more aggressive regex to strip all whitespace including non-breaking spaces and zero-width characters
                const currentTitle = text.replace(/[\s\u200B\u00A0\uFEFF\u2000-\u200F\u2028\u2029]/g, '');
                console.log(`[DEBUG_LOG] ${e.key} pressed. text:`, JSON.stringify(text), 'currentTitle after regex:', JSON.stringify(currentTitle));
                const isTitleEmpty = currentTitle === '';
                if (isTitleEmpty) {
                    console.log(`[DEBUG_LOG] isTitleEmpty is true, calling onDeleteTask from ${e.key}`);
                    e.preventDefault();
                    onDeleteTask(e);
                }
            }
        };

        const onDeleteTask = async (e) => {
            console.log('[DEBUG_LOG] onDeleteTask entered. e.target.textContent:', e?.target ? JSON.stringify(e.target.textContent) : 'N/A');
            // If the title is NOT empty, ask for confirmation
            // If it is empty, just delete it (likely a newly created but unwanted sibling/subtask)
            const targetTitle = (e?.target?.textContent || '').replace(/[\s\u200B\u00A0\uFEFF\u2000-\u200F\u2028\u2029]/g, '');
            const propTitle = (props.task.title || '').replace(/[\s\u200B\u00A0\uFEFF\u2000-\u200F\u2028\u2029]/g, '');
            const isTitleEmpty = targetTitle === '' || propTitle === '';
            
            console.log('[DEBUG_LOG] onDeleteTask - isTitleEmpty:', isTitleEmpty, 'targetTitle:', JSON.stringify(targetTitle), 'propTitle:', JSON.stringify(propTitle));
            
            if (isTitleEmpty || confirm(`Are you sure you want to delete "${e?.target?.textContent?.trim() || props.task.title}"?`)) {
                try {
                    console.log('[DEBUG_LOG] Proceeding with deleteTask for ID:', props.task.id);
                    await deleteTask(props.task.id);
                    emit('refresh');
                } catch (err) {
                    console.error('Error during deleteTask:', err);
                }
            }
        };

        const onUpdateTitle = async (e) => {
            if (!props.task) return;
            const text = e.target.textContent || '';
            const newTitle = text.trim();
            if (newTitle === (props.task.title || '').trim()) return;
            
            console.log('[DEBUG_LOG] Updating task title:', props.task.id, 'New Title:', JSON.stringify(newTitle));
            const { subtasks, ...taskWithoutSubtasks } = props.task;
            await updateTask(props.task.id, { ...taskWithoutSubtasks, title: newTitle });
            emit('refresh');
        };

        const onPaste = async (e) => {
            if (!props.task) return;
            
            // If the title is not empty, we don't do the multi-line split logic
            // The requirement says "into a an empty/new task name field"
            const currentText = e.target.textContent || '';
            if (currentText.trim() !== '') return;

            const pastedText = (e.clipboardData || window.clipboardData).getData('text');
            if (!pastedText) return;

            const lines = pastedText.split(/\r?\n/).map(l => l.trim()).filter(l => l !== '');
            
            if (lines.length > 1) {
                e.preventDefault();
                const createMultiple = confirm(`You pasted ${lines.length} lines. Do you want to create a separate task for each line?`);
                
                if (createMultiple) {
                    // Update current task with the first line
                    const { subtasks, ...taskWithoutSubtasks } = props.task;
                    await updateTask(props.task.id, { ...taskWithoutSubtasks, title: lines[0] });
                    
                    // Create siblings for the rest of the lines
                    const defaultStatusId = props.projectStatuses?.length > 0 ? props.projectStatuses[0].id : null;
                    let lastId = props.task.id;
                    for (let i = 1; i < lines.length; i++) {
                        const newSibling = await addSibling(lastId, lines[i], defaultStatusId);
                        lastId = newSibling.id;
                    }
                    emit('refresh');
                } else {
                    // Just paste normally as one task (but we prevented default, so we set it manually)
                    const singleLineText = lines.join(' ');
                    e.target.textContent = singleLineText;
                    // Trigger update
                    await onUpdateTitle({ target: e.target });
                }
            }
        };

        const onUpdateEstimate = async (e) => {
            if (!props.task) return;
            const newEstimate = parseEstimate(e.target.value);
            const { subtasks, ...taskWithoutSubtasks } = props.task;
            await updateTask(props.task.id, { ...taskWithoutSubtasks, estimateMinutes: newEstimate });
            isEditingEstimate.value = false;
            emit('refresh');
        };

        const startEditingEstimate = () => {
            isEditingEstimate.value = true;
        };

        const onUpdateDate = async (field, newVal) => {
            if (!props.task) return;
            const { subtasks, ...taskWithoutSubtasks } = props.task;
            await updateTask(props.task.id, { ...taskWithoutSubtasks, [field]: newVal });
            emit('refresh');
        };

        const onUpdatePriorityId = async (priorityId) => {
            if (!props.task) return;
            const { subtasks, ...taskWithoutSubtasks } = props.task;
            await updateTask(props.task.id, { ...taskWithoutSubtasks, priorityId: priorityId });
            emit('refresh');
        };

        const onUpdateStatus = async (e) => {
            if (!props.task) return;
            const newStatusId = e.target.value;
            if (!newStatusId) return;
            const newStatus = props.projectStatuses?.find(s => s.id === newStatusId);
            const { subtasks, ...taskWithoutSubtasks } = props.task;
            await updateTask(props.task.id, { ...taskWithoutSubtasks, statusId: newStatusId, isCompleted: newStatus?.isCompletedState || false });
            emit('refresh');
        };

        const onUpdateTaskType = async (e) => {
            if (!props.task) return;
            const newTaskTypeId = e.target.value || null;
            const { subtasks, ...taskWithoutSubtasks } = props.task;
            await updateTask(props.task.id, { ...taskWithoutSubtasks, taskTypeId: newTaskTypeId });
            emit('refresh');
        };

        const onOpenTask = () => {
            emit('open-task', props.task, props.projectStatuses, props.projectTaskTypes, props.projectPriorities);
        };

        const onContextMenu = (e) => {
            emit('context-menu', e, props.task);
        };

        onMounted(() => {
            if (props.task && (props.task.title === "" || props.task.title === null)) {
                setTimeout(() => {
                    if (titleElement.value) {
                        titleElement.value.focus();
                        // Move cursor to end
                        const range = document.createRange();
                        const sel = window.getSelection();
                        range.selectNodeContents(titleElement.value);
                        range.collapse(false);
                        sel.removeAllRanges();
                        sel.addRange(range);
                    }
                }, 50);
            }
        });

        return {
            getPriorityName,
            getStatusName,
            getStatusColor,
            getPriorityIcon,
            getPriorityColor,
            onDragStart,
            onCheckboxMouseDown,
            onCheckboxClick,
            onDragOver,
            onDragLeave,
            onDrop,
            onDeleteTask,
            onUpdateTitle,
            onUpdateEstimate,
            isSelected,
            startEditingEstimate,
            isEditingEstimate,
            onUpdateDate,
            onUpdatePriorityId,
            onUpdateStatus,
            onPaste,
            onUpdateTaskType,
            getTaskTypeName,
            getTaskTypeColor,
            getTaskTypeIcon,
            onOpenTask,
            onContextMenu,
            onTitleEsc,
            onGeneralKeyDown,
            onTitleKeyDown,
            getSortedSubtasks,
            titleElement,
            formatFriendlyDate,
            formatForInput,
            formatEstimate,
            dropClass
        };
    }
};
</script>

<style scoped>
.task-row-container {
    display: flex;
    flex-direction: column;
    width: 100%;
}

.tasks-row {
    display: grid;
    background-color: var(--bg-dark);
    transition: background-color 0.2s;
    border-bottom: 1px solid var(--border-primary);
    overflow: visible;
}

.tasks-row:hover {
    background-color: var(--bg-card);
}

.tasks-cell {
    padding: 0.5rem;
    display: flex;
    align-items: center;
    min-width: 0;
    font-size: 0.875rem;
    position: relative;
}

.task-title-container {
    gap: 8px;
}

.subtask-indent {
}

.task-title {
    flex-grow: 1;
    outline: none;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.status-badge, .priority-badge, .type-badge {
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 0.875rem;
    font-weight: 600;
    color: white;
    white-space: nowrap;
    background-color: transparent;
    display: inline-flex;
    align-items: center;
    gap: 4px;
    text-decoration: none;
}

.dropdown-toggle::after {
    display: none;
}

.type-badge {
}

.priority-badge {
}

.priority-Lowest { }
.priority-Low { }
.priority-Medium { }
.priority-High { }
.priority-Highest { }
.priority-Critical { background-color: #ff0000; box-shadow: 0 0 5px rgba(255,0,0,0.5); }

.dropdown {
    position: relative;
    display: inline-block;
    width: 100%;
}

.type-badge, .status-badge, .priority-badge {
    width: 100%;
    justify-content: flex-start;
}

.dropdown-menu {
    z-index: 1050;
    margin: 0 !important;
    max-height: 250px;
    overflow-y: auto;
}

.dropdown-item {
    cursor: pointer;
}

.date-cell {
    font-variant-numeric: tabular-nums;
    color: var(--text-muted);
    padding: 2px 4px !important;
    overflow: visible !important;
}

.estimate-cell {
    font-variant-numeric: tabular-nums;
    color: var(--text-muted);
}

.clickable {
    cursor: pointer;
}

.hidden-date-picker {
    position: absolute;
    visibility: hidden;
    width: 0;
    height: 0;
}

.estimate-input {
    width: 100%;
    background: var(--bg-darker);
    border: 1px solid var(--accent-blue);
    color: var(--text-primary);
    padding: 2px 4px;
    border-radius: 4px;
    font-size: 0.875rem;
}

.drag-over-before { border-top: 2px solid var(--accent-blue) !important; }
.drag-over-after { border-bottom: 2px solid var(--accent-blue) !important; }
.drag-over-inside { background-color: rgba(88, 166, 255, 0.1) !important; }

.action-link-icon {
    display: inline-block;
    width: 14px;
    height: 14px;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='currentColor' class='bi bi-box-arrow-up-right' viewBox='0 0 16 16'%3E%3Cpath fill-rule='evenodd' d='M8.636 3.5a.5.5 0 0 0-.5-.5H1.5A1.5 1.5 0 0 0 0 4.5v10A1.5 1.5 0 0 0 1.5 16h10a1.5 1.5 0 0 0 1.5-1.5V7.864a.5.5 0 0 0-1 0V14.5a.5.5 0 0 1-.5.5h-10a.5.5 0 0 1-.5-.5v-10a.5.5 0 0 1 .5-.5h6.636a.5.5 0 0 0 .5-.5z'/%3E%3Cpath fill-rule='evenodd' d='M16 .5a.5.5 0 0 0-.5-.5h-5a.5.5 0 0 0 0 1h3.793L6.146 9.146a.5.5 0 1 0 .708.708L15 1.707V5.5a.5.5 0 0 0 1 0v-5z'/%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-size: contain;
    opacity: 0.6;
}

.action-link-icon:hover {
    opacity: 1;
}
</style>
