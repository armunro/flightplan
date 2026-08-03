<template>
    <div class="task-row-container" :class="[{ 'is-selected': isSelected }, themeClass]" :data-task-id="task?.id">
        <div class="tasks-row" 
             draggable="true" 
             @dragstart="onDragStart"
             @dragover="onDragOver"
             @dragenter="onDragEnter"
             @dragleave="onDragLeave"
             @drop="onDrop"
             @contextmenu.prevent="onContextMenu"
             @dblclick="onOpenTask"
             :class="[dropClass, { 'has-subtasks': task && task.subtasks && task.subtasks.length > 0, 'is-last-row': isLast && getSortedSubtasks(task.subtasks).length === 0 }]"
             :style="gridStyle">
            <div class="tasks-cell selection-cell">
                <input type="checkbox" :checked="isSelected" @mousedown="onCheckboxMouseDown($event)" @click.stop="onCheckboxClick($event)">
            </div>
            
            <template v-for="colId in visibleColumnIds" :key="colId">
                <div v-if="colId === 'type'" class="tasks-cell type-column">
                    <div class="dropdown" v-if="task" :class="{ 'dropup': isLast }">
                        <span class="type-badge dropdown-toggle" 
                            data-bs-toggle="dropdown"
                            data-bs-auto-close="outside"
                            :style="{ color: getTaskTypeColor(task.taskTypeId) }">
                            <i :class="getTaskTypeIcon(task.taskTypeId)"></i>
                            <span class="ms-1">{{ getTaskTypeName(task.taskTypeId) }}</span>
                        </span>
                        <ul class="dropdown-menu border-secondary shadow" :class="{ 'dropdown-menu-dark': theme === 'Cosmic' }">
                            <li><a class="dropdown-item" href="#" @click.prevent="onUpdateTaskType({ target: { value: null } })">-- Type --</a></li>
                            <li v-for="t in projectTaskTypes" :key="t.id">
                                <a class="dropdown-item" href="#" @click.prevent="onUpdateTaskType({ target: { value: t.id } })" :style="{ color: t.color }">
                                    <i :class="t.icon" class="me-2"></i>{{ t.name }}
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
                
                <div v-else-if="colId === 'name'" class="tasks-cell task-title-container name-column" 
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
                    <a v-if="task?.link" :href="task.link" target="_blank" class="action-link-icon ms-1" title="Open link">
                        <i class="bi bi-box-arrow-up-right"></i>
                    </a>
                    <span v-if="task?.description" class="description-indicator ms-1" title="Task has description">
                        <i class="bi bi-text-paragraph"></i>
                    </span>
                </div>

                <div v-else-if="colId === 'status'" class="tasks-cell status-column">
                    <div class="dropdown" v-if="task" :class="{ 'dropup': isLast }">
                        <span class="status-badge dropdown-toggle" 
                            data-bs-toggle="dropdown"
                            data-bs-auto-close="outside"
                            :style="{ color: getStatusColor(task.statusId) }">
                            <i class="bi bi-circle-fill" style="font-size: 8px; margin-right: 4px;"></i>
                            <span>{{ getStatusName(task.statusId) }}</span>
                        </span>
                        <ul class="dropdown-menu border-secondary shadow" :class="{ 'dropdown-menu-dark': theme === 'Cosmic' }">
                            <li v-for="s in projectStatuses" :key="s.id">
                                <a class="dropdown-item" href="#" @click.prevent="onUpdateStatus({ target: { value: s.id } })" :style="{ color: s.color }">
                                    <i class="bi bi-circle-fill me-2" style="font-size: 8px;"></i>{{ s.name }}
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>

                <div v-else-if="colId === 'priority'" class="tasks-cell priority-column">
                    <div class="dropdown" v-if="task" :class="{ 'dropup': isLast }">
                        <span class="priority priority-badge dropdown-toggle" 
                            data-bs-toggle="dropdown"
                            data-bs-auto-close="outside"
                            :style="{ color: getPriorityColor(task.priorityId) }">
                            <i :class="getPriorityIcon(task.priorityId)" style="margin-right: 4px;"></i>
                            <span>{{ getPriorityName(task.priorityId) }}</span>
                        </span>
                        <ul class="dropdown-menu border-secondary shadow" :class="{ 'dropdown-menu-dark': theme === 'Cosmic' }">
                            <li v-for="p in projectPriorities" :key="p.id">
                                <a class="dropdown-item" href="#" @click.prevent="onUpdatePriorityId(p.id)" :style="{ color: p.color }">
                                    <i :class="p.icon" class="me-2"></i>{{ p.name }}
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>

                <div v-else-if="colId === 'start'" class="tasks-cell date-cell start-column">
                    <date-time-selector 
                        :model-value="task?.start" 
                        placeholder="Start"
                        :is-closed="task?.isCompleted"
                        @update:model-value="onUpdateDate('start', $event)"
                        size="small"
                    />
                </div>

                <div v-else-if="colId === 'end'" class="tasks-cell date-cell end-column">
                    <date-time-selector 
                        :model-value="task?.end" 
                        placeholder="End"
                        :is-closed="task?.isCompleted"
                        @update:model-value="onUpdateDate('end', $event)"
                        size="small"
                    />
                </div>

                <div v-else-if="colId === 'estimate'" class="tasks-cell estimate-cell clickable estimate-column" @click="startEditingEstimate">
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
                <div v-else><!-- Fallback for unknown columns --></div>
            </template>

            <div class="tasks-cell"></div>
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
                          :theme="theme"
                          :visible-column-ids="visibleColumnIds"
                          @refresh="$emit('refresh')" 
                          @open-task="$emit('open-task', $event)"
                          @toggle-select="$emit('toggle-select', $event)"
                          @context-menu="(e, task) => $emit('context-menu', e, task)"></task-row>
            </template>
        </template>
    </div>
</template>

<script>
import { ref, computed, onMounted, watch, nextTick } from 'vue';
import DateTimeSelector from './DateTimeSelector.vue';
import { updateTask, addSubtask, addSibling, deleteTask, moveTask, copyTask } from '../js/tasks-api';
import { formatFriendlyDate, formatForInput, formatToISO, formatEstimate, parseEstimate } from '../js/utils';

export default {
    name: 'TaskRow',
    components: {
        DateTimeSelector
    },
    props: ['task', 'depth', 'projectStatuses', 'projectTaskTypes', 'projectPriorities', 'showClosed', 'gridStyle', 'isLast', 'selectedTaskIds', 'parentTaskId', 'previousTaskId', 'theme', 'visibleColumnIds'],
    emits: ['refresh', 'open-task', 'context-menu', 'toggle-select'],
    setup(props, { emit }) {
        const isColumnVisible = (columnId) => {
            if (!props.visibleColumnIds) return true;
            return props.visibleColumnIds.includes(columnId);
        };

        const visibleColumnIds = computed(() => props.visibleColumnIds);
        const themeClass = computed(() => `theme-${(props.theme || 'Cosmic').toLowerCase()}`);
        const dropPosition = ref(null); // 'before', 'after', 'inside'
        const isEditingEstimate = ref(false);
        const titleElement = ref(null);

        const isSelected = computed(() => props.selectedTaskIds?.includes(props.task.id));

        const getSortedSubtasks = (subtasks) => {
            if (!subtasks) return [];
            return subtasks.filter(s => props.showClosed || !s.isCompleted);
        };

        onMounted(() => {
            console.log('[DEBUG_LOG] TaskRow onMounted:', props.task?.id);
            handleFocus();
        });

        watch(() => props.task.id, (newId, oldId) => {
            console.log('[DEBUG_LOG] TaskRow watch task.id changed from', oldId, 'to', newId);
            handleFocus();
        });

        const handleFocus = () => {
            if (props.task) {
                // Only skip re-focus if it's a title update AND we don't have an explicit focus request
                if (window._lastUpdatedTaskId === props.task.id && window._focusTaskId !== props.task.id) {
                    console.log('[DEBUG_LOG] handleFocus: skipping re-focus for title update', props.task.id);
                    window._lastUpdatedTaskId = null;
                    return;
                }
                
                // Clear the update flag if we matched
                if (window._lastUpdatedTaskId === props.task.id) {
                    window._lastUpdatedTaskId = null;
                }

                // Only auto-focus if we explicitly triggered a "new task" creation, deletion, or movement for THIS specific ID
                if (window._focusTaskId === props.task.id) {
                    console.log('[DEBUG_LOG] handleFocus: MATCH for task:', props.task.id);
                    console.log('[DEBUG_LOG] handleFocus: Focus match found for task:', props.task.id, 'window._isDeletingTaskId:', window._isDeletingTaskId);
                    // Prevent this component instance from stealing focus if it's being deleted
                    // The "watch" or "onMounted" might trigger on the old component just before it's unmounted
                    if (window._isDeletingTaskId === props.task.id) {
                        console.log('[DEBUG_LOG] handleFocus: skipping focus because task is being deleted', props.task.id);
                        return;
                    }

                    console.log('[DEBUG_LOG] handleFocus: Focus match found for task:', props.task.id);
                    
                    nextTick(() => {
                        // Use a slightly longer delay for the first attempt to allow DOM to settle after refresh
                        let attempts = 0;
                        const maxAttempts = 15;
                        const tryFocus = () => {
                            attempts++;
                            const el = titleElement.value;
                            if (el && typeof el.focus === 'function') {
                                // Check if the element is actually "ready" (visible and in DOM)
                                const isVisible = el.offsetWidth > 0 || el.offsetHeight > 0 || el.getClientRects().length > 0;
                                if (!isVisible) {
                                    if (attempts < maxAttempts) {
                                        setTimeout(tryFocus, 50);
                                    } else {
                                        window._focusTaskId = null;
                                    }
                                    return;
                                }

                                // Force focusable just in case it's being stubborn
                                if (el.getAttribute('contenteditable') !== 'true') {
                                    el.setAttribute('contenteditable', 'true');
                                }
                                
                                console.log('[DEBUG_LOG] handleFocus: actual focus() attempt for:', props.task.id);
                                el.focus();
                                // For contenteditable, sometimes we need to ensure the cursor is at the end
                                try {
                                    const range = document.createRange();
                                    const sel = window.getSelection();
                                    range.selectNodeContents(el);
                                    range.collapse(false);
                                    sel.removeAllRanges();
                                    sel.addRange(range);
                                } catch (e) {
                                    console.error('[DEBUG_LOG] handleFocus: error setting cursor position:', e);
                                }
                                
                                console.log('[DEBUG_LOG] handleFocus: focus() called on element for:', props.task.id, 'window._focusTaskId was:', window._focusTaskId);
                                
                                // Monitor if focus is lost immediately
                                setTimeout(() => {
                                    if (document.activeElement !== el) {
                                        console.warn('[DEBUG_LOG] handleFocus: focus lost immediately after setting for:', props.task.id, 'activeElement:', document.activeElement.tagName);
                                    } else {
                                        console.log('[DEBUG_LOG] handleFocus: focus confirmed for:', props.task.id);
                                        // Clear the flag only after we've confirmed focus is stable
                                        window._focusTaskId = null;
                                        // Also clear last updated if it was this task
                                        if (window._lastUpdatedTaskId === props.task.id) {
                                            window._lastUpdatedTaskId = null;
                                        }
                                    }
                                }, 100);

                                // Move cursor to end
                                try {
                                    const range = document.createRange();
                                    const sel = window.getSelection();
                                    range.selectNodeContents(el);
                                    range.collapse(false);
                                    sel.removeAllRanges();
                                    sel.addRange(range);
                                } catch (e) {
                                    // Ignore selection errors
                                }
                            } else if (attempts < maxAttempts) {
                                // Fallback: try to find it in the DOM if ref is failing us
                                if (!el || typeof el.focus !== 'function') {
                                    const selector = `.task-row-container[data-task-id="${props.task.id}"] .task-title`;
                                    const domEl = document.querySelector(selector);
                                    if (domEl && typeof domEl.focus === 'function') {
                                        titleElement.value = domEl;
                                    }
                                }
                                setTimeout(tryFocus, 50);
                            } else {
                                window._focusTaskId = null;
                            }
                        };
                        
                        // Start with a small delay
                        setTimeout(tryFocus, 50);
                    });
                }
            }
        };

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

        const dragCounter = ref(0);

        const onDragStart = (e) => {
            console.log('[DEBUG_LOG] Task drag start:', props.task.id);
            // Use unique MIME types to avoid ambiguity during dragover
            const taskMimeType = 'application/x-flightplan-task';
            e.dataTransfer.setData(taskMimeType, props.task.id);
            e.dataTransfer.setData('text/plain', props.task.id);
            e.dataTransfer.setData('taskId', props.task.id); // Keeping for backward compatibility if needed
            e.dataTransfer.effectAllowed = 'copyMove';
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
            
            e.dataTransfer.dropEffect = e.ctrlKey ? 'copy' : 'move';
            
            const rect = e.currentTarget.getBoundingClientRect();
            const y = e.clientY - rect.top;
            const threshold = rect.height / 3;

            let newPosition = null;
            if (y < threshold) {
                newPosition = 'before';
            } else if (y > rect.height - threshold && props.isLast) {
                newPosition = 'after';
            } else {
                newPosition = 'inside';
            }

            if (dropPosition.value !== newPosition) {
                dropPosition.value = newPosition;
            }
        };

        const onDragEnter = (e) => {
            const types = Array.from(e.dataTransfer.types);
            const isDraggingTask = types.some(t => t.toLowerCase() === 'application/x-flightplan-task' || t.toLowerCase() === 'taskid');
            if (!isDraggingTask) return;
            
            e.preventDefault();
        };

        const onDragLeave = (e) => {
            if (!e.currentTarget.contains(e.relatedTarget)) {
                dropPosition.value = null;
            }
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
                if (e.ctrlKey) {
                    await copyTask(draggedTaskId, null, props.task.id, positionMap[pos]);
                } else {
                    await moveTask(draggedTaskId, null, props.task.id, positionMap[pos]);
                }
                emit('refresh');
            }
        };

        const dropClass = computed(() => {
            if (!dropPosition.value) return '';
            return `drag-over-${dropPosition.value}`;
        });


        const onTitleKeyDown = async (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                e.stopPropagation();
                // If Enter is pressed, create a new sibling task
                const defaultStatusId = props.projectStatuses?.length > 0 ? props.projectStatuses[0].id : null;
                try {
                    // Ensure the current title is saved before creating sibling
                    // Pass false to avoid triggering refresh before sibling creation
                    await onUpdateTitle(e, false);
                    
                    const newSibling = await addSibling(props.task.id, "", defaultStatusId); 
                    
                    // Set flag for new task creation with specific ID
                    if (newSibling && newSibling.id) {
                        window._focusTaskId = newSibling.id;
                    }
                    
                    emit('refresh');
                } catch (err) {
                    window._focusTaskId = null;
                }
            }
        };

        const onTitleTabKeyDown = async (e) => {
            e.preventDefault();
            e.stopPropagation();

            try {
                // Ensure the current title is saved before moving
                await onUpdateTitle(e);

                if (e.shiftKey) {
                    // Shift+Tab: Promote task (outdent)
                    if (props.parentTaskId) {
                        window._focusTaskId = props.task.id;
                        await moveTask(props.task.id, null, props.parentTaskId, 'After'); // Shift+Tab: Promote task (outdent)
                        emit('refresh');
                    }
                } else {
                    // Tab: Demote task (indent)
                    if (props.previousTaskId) {
                        window._focusTaskId = props.task.id;
                        await moveTask(props.task.id, null, props.previousTaskId, 'Inside'); // Tab: Demote task (indent)
                        emit('refresh');
                    }
                }
            } catch (err) {
                window._focusTaskId = null;
            }
        };

        const onTitleEsc = (e) => {
            e.target.innerText = props.task.title;
            e.target.blur();
        };

        const onGeneralKeyDown = (e) => {
            if (e.key === 'Enter') {
                // Enter is handled by onTitleKeyDown, but we want to make sure it doesn't propagate to any general handlers
                return;
            }
            if (e.key === 'Tab') {
                e.preventDefault();
                e.stopPropagation();
                onTitleTabKeyDown(e);
            } else if (e.key === 'Delete' || e.key === 'Backspace') {
                const text = e.target.textContent || '';
                // Use a more aggressive regex to strip all whitespace including non-breaking spaces and zero-width characters
                const currentTitle = text.replace(/[\s\u200B\u00A0\uFEFF\u2000-\u200F\u2028\u2029]/g, '');
                const isTitleEmpty = currentTitle === '';
                if (isTitleEmpty) {
                    console.log('[DEBUG_LOG] Backspace/Delete on empty title, triggering onDeleteTask');
                    e.preventDefault();
                    e.stopPropagation();
                    onDeleteTask(e);
                }
            }
        };

        const onDeleteTask = async (e) => {
            // If the title is NOT empty, ask for confirmation
            // If it is empty, just delete it (likely a newly created but unwanted sibling/subtask)
            const targetTitle = (e?.target?.textContent || '').replace(/[\s\u200B\u00A0\uFEFF\u2000-\u200F\u2028\u2029]/g, '');
            const propTitle = (props.task.title || '').replace(/[\s\u200B\u00A0\uFEFF\u2000-\u200F\u2028\u2029]/g, '');
            const isTitleEmpty = targetTitle === '' || propTitle === '';
            
            if (isTitleEmpty || confirm(`Are you sure you want to delete "${e?.target?.textContent?.trim() || props.task.title}"?`)) {
                try {
                    if (isTitleEmpty) {
                        const allVisibleTasks = Array.from(document.querySelectorAll('.task-row-container'));
                        const currentIndex = allVisibleTasks.findIndex(el => el.getAttribute('data-task-id') === props.task.id);
                        
                        if (currentIndex > 0) {
                            const visuallyAboveTask = allVisibleTasks[currentIndex - 1];
                            const visuallyAboveTaskId = visuallyAboveTask.getAttribute('data-task-id');
                            console.log('[DEBUG_LOG] onDeleteTask: visually preceding task found at index', currentIndex - 1, 'ID:', visuallyAboveTaskId);
                            if (visuallyAboveTaskId) {
                                window._focusTaskId = visuallyAboveTaskId;
                            }
                        } else {
                            console.log('[DEBUG_LOG] onDeleteTask: No visually preceding task found (currentIndex 0), checking for parent or previous list');
                            if (props.parentTaskId) {
                                console.log('[DEBUG_LOG] onDeleteTask: setting focus to parent:', props.parentTaskId);
                                window._focusTaskId = props.parentTaskId;
                            } else {
                                const currentList = e.target.closest('.list');
                                if (currentList) {
                                    const allLists = Array.from(document.querySelectorAll('.list'));
                                    const listIndex = allLists.indexOf(currentList);
                                    if (listIndex > 0) {
                                        const prevList = allLists[listIndex - 1];
                                        const allTasksInPrevList = Array.from(prevList.querySelectorAll('.task-row-container'));
                                        if (allTasksInPrevList.length > 0) {
                                            const lastTaskInPrevList = allTasksInPrevList[allTasksInPrevList.length - 1];
                                            const lastTaskId = lastTaskInPrevList.getAttribute('data-task-id');
                                            if (lastTaskId) {
                                                console.log('[DEBUG_LOG] onDeleteTask: setting focus to last task of previous list:', lastTaskId);
                                                window._focusTaskId = lastTaskId;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    window._isDeletingTaskId = props.task.id;
                    console.log('[DEBUG_LOG] onDeleteTask: calling deleteTask for', props.task.id, 'window._focusTaskId is', window._focusTaskId);
                    await deleteTask(props.task.id);
                    // Clear last updated and deleting flags to avoid conflict
                    window._lastUpdatedTaskId = null;
                    window._isDeletingTaskId = null;
                    console.log('[DEBUG_LOG] onDeleteTask: emitting refresh, window._focusTaskId still', window._focusTaskId);
                    emit('refresh');
                } catch (err) {
                    window._isDeletingTaskId = null;
                    console.error('Error during deleteTask:', err);
                }
            }
        };

        const onUpdateTitle = async (e, shouldRefresh = true) => {
            if (!props.task) return;
            const text = e.target.textContent || '';
            const newTitle = text.trim();
            if (newTitle === (props.task.title || '').trim()) return;
            
            console.log('[DEBUG_LOG] Updating task title:', props.task.id, 'New Title:', JSON.stringify(newTitle));
            const { subtasks, ...taskWithoutSubtasks } = props.task;
            await updateTask(props.task.id, { ...taskWithoutSubtasks, title: newTitle });
            // Set a flag to prevent re-focusing on mount if it's just a title update
            window._lastUpdatedTaskId = props.task.id;
            
            // If we have a pending focus request for THIS task, we should keep it
            // because onUpdateTitle might be called just before a refresh that we WANT focus for
            // (like when pressing Tab)
            
            if (shouldRefresh) {
                emit('refresh');
            }
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
            isColumnVisible,
            visibleColumnIds,
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
            dropClass,
            themeClass
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

.tasks-row.is-last-row {
    border-bottom: none;
    border-bottom-left-radius: 8px;
    border-bottom-right-radius: 8px;
}

.tasks-row:hover {
    background-color: var(--bg-card);
}

.tasks-cell {
    padding: 0.5rem;
    display: flex;
    align-items: center;
    min-width: 0;
    font-size: var(--fs-sm);
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
    font-size: var(--fs-sm);
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
    font-size: var(--fs-sm);
}

.drag-over-inside { background-color: rgba(88, 166, 255, 0.1) !important; }

.action-link-icon, .description-indicator {
    opacity: 0.7;
    color: var(--text-muted);
    cursor: pointer;
    transition: opacity 0.2s;
}

.action-link-icon:hover, .description-indicator:hover {
    opacity: 1;
}
</style>
