import {defineStore} from 'pinia'
import {ref} from "vue";

export const typesStore = defineStore('types', {
    state: () => {
        return {
            waypointTypes: [
                {
                    type: 'Task',
                    category: 'Action-Oriented',
                    description: 'A concrete item to complete',
                    icon: 'fa-check-square'
                },
                {
                    type: 'Errand',
                    category: 'Action-Oriented',
                    description: 'A location-based or physical-world task',
                    icon: 'fa-shopping-basket'
                },
                {
                    type: 'Follow-Up',
                    category: 'Action-Oriented',
                    description: 'A reminder to check back or ping someone',
                    icon: 'fa-reply'
                },
                {
                    type: 'Delegation',
                    category: 'Action-Oriented',
                    description: 'A task you\'ve handed off to someone else',
                    icon: 'fa-user-tag'
                },
                {
                    type: 'Review',
                    category: 'Action-Oriented',
                    description: 'A block of time to review or audit',
                    icon: 'fa-clipboard-check'
                },
                {
                    type: 'Email',
                    category: 'Communication',
                    description: 'A message to send or read',
                    icon: 'fa-envelope'
                },
                {type: 'Phone', category: 'Communication', description: 'A call to make or expect', icon: 'fa-phone'},
                {
                    type: 'Meeting',
                    category: 'Communication',
                    description: 'A scheduled conversation or gathering',
                    icon: 'fa-users'
                },
                {
                    type: 'Appointment',
                    category: 'Communication',
                    description: 'A scheduled personal or external event',
                    icon: 'fa-calendar-check'
                },
                {
                    type: 'Checkpoint',
                    category: 'Planning & Strategy',
                    description: 'A key milestone in a longer process',
                    icon: 'fa-flag-checkered'
                },
                {
                    type: 'Deadline',
                    category: 'Planning & Strategy',
                    description: 'A hard end time or due moment',
                    icon: 'fa-hourglass-end'
                },
                {
                    type: 'Sync Point',
                    category: 'Planning & Strategy',
                    description: 'Where multiple tasks or people align',
                    icon: 'fa-link'
                },
                {
                    type: 'Brainstorm',
                    category: 'Planning & Strategy',
                    description: 'Creative or ideation time',
                    icon: 'fa-lightbulb'
                },
                {
                    type: 'Focus Session',
                    category: 'Mental & Emotional',
                    description: 'Dedicated time for deep work',
                    icon: 'fa-bullseye'
                },
                {
                    type: 'Break',
                    category: 'Mental & Emotional',
                    description: 'Rest, recovery, or reset',
                    icon: 'fa-mug-saucer'
                },
                {
                    type: 'Mood Check',
                    category: 'Mental & Emotional',
                    description: 'Log or reflect on emotional state',
                    icon: 'fa-face-smile'
                },
                {
                    type: 'Motivation',
                    category: 'Mental & Emotional',
                    description: 'Inspiration, affirmation, or self-talk',
                    icon: 'fa-quote-left'
                },
                {
                    type: 'Note',
                    category: 'Information & Notes',
                    description: 'A textual note or idea',
                    icon: 'fa-sticky-note'
                },
                {
                    type: 'Reference',
                    category: 'Information & Notes',
                    description: 'A saved link, file, or document',
                    icon: 'fa-bookmark'
                },
                {
                    type: 'Journal',
                    category: 'Information & Notes',
                    description: 'A personal reflection or entry',
                    icon: 'fa-pen-fancy'
                },
                {
                    type: 'Transit',
                    category: 'Location-Based',
                    description: 'Movement from one location to another',
                    icon: 'fa-route'
                },
                {
                    type: 'Visit',
                    category: 'Location-Based',
                    description: 'A stop at a specific place',
                    icon: 'fa-map-marker-alt'
                },
                {
                    type: 'Location Alert',
                    category: 'Location-Based',
                    description: 'Triggered on arrival at a specific place',
                    icon: 'fa-bell-on'
                },
                {
                    type: 'Reminder',
                    category: 'Digital & System',
                    description: 'A ping or alert of any kind',
                    icon: 'fa-bell'
                },
                {
                    type: 'Form Submission',
                    category: 'Digital & System',
                    description: 'Time to fill or check a form',
                    icon: 'fa-file-signature'
                },
                {
                    type: 'System Check',
                    category: 'Digital & System',
                    description: 'Scheduled review of automated systems',
                    icon: 'fa-server'
                },
                {
                    type: 'Content Publish',
                    category: 'Digital & System',
                    description: 'Time to post or publish something',
                    icon: 'fa-share-square'
                },
            ]
            ,
            spanTypes: [{
                name: 'Work',
                icon: 'fa-paper-plane'
            }, {
                name: 'Transit',
                icon: 'fa-paper-plane'
            }]
        }
    },
    actions: {},
})
