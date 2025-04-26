import {defineStore} from 'pinia'
import {ref} from "vue";

export const typesStore = defineStore('types', {
    state: () => {
        return {
            waypointTypes: [
                {
                    name: 'Task',
                    category: 'Action-Oriented',
                    description: 'A concrete item to complete',
                    icon: 'fa-check-square'
                },
                {
                    name: 'Errand',
                    category: 'Action-Oriented',
                    description: 'A location-based or physical-world task',
                    icon: 'fa-shopping-basket'
                },
                {
                    name: 'Follow-Up',
                    category: 'Action-Oriented',
                    description: 'A reminder to check back or ping someone',
                    icon: 'fa-reply'
                },
                {
                    name: 'Delegation',
                    category: 'Action-Oriented',
                    description: 'A task you\'ve handed off to someone else',
                    icon: 'fa-user-tag'
                },
                {
                    name: 'Review',
                    category: 'Action-Oriented',
                    description: 'A block of time to review or audit',
                    icon: 'fa-clipboard-check'
                },
                {
                    name: 'Email',
                    category: 'Communication',
                    description: 'A message to send or read',
                    icon: 'fa-envelope'
                },
                {name: 'Phone', category: 'Communication', description: 'A call to make or expect', icon: 'fa-phone'},
                {
                    name: 'Meeting',
                    category: 'Communication',
                    description: 'A scheduled conversation or gathering',
                    icon: 'fa-users'
                },
                {
                    name: 'Appointment',
                    category: 'Communication',
                    description: 'A scheduled personal or external event',
                    icon: 'fa-calendar-check'
                },
                {
                    name: 'Checkpoint',
                    category: 'Planning & Strategy',
                    description: 'A key milestone in a longer process',
                    icon: 'fa-flag-checkered'
                },
                {
                    name: 'Deadline',
                    category: 'Planning & Strategy',
                    description: 'A hard end time or due moment',
                    icon: 'fa-hourglass-end'
                },
                {
                    name: 'Sync Point',
                    category: 'Planning & Strategy',
                    description: 'Where multiple tasks or people align',
                    icon: 'fa-link'
                },
                {
                    name: 'Brainstorm',
                    category: 'Planning & Strategy',
                    description: 'Creative or ideation time',
                    icon: 'fa-lightbulb'
                },
                {
                    name: 'Focus Session',
                    category: 'Mental & Emotional',
                    description: 'Dedicated time for deep work',
                    icon: 'fa-bullseye'
                },
                {
                    name: 'Break',
                    category: 'Mental & Emotional',
                    description: 'Rest, recovery, or reset',
                    icon: 'fa-mug-saucer'
                },
                {
                    name: 'Mood Check',
                    category: 'Mental & Emotional',
                    description: 'Log or reflect on emotional state',
                    icon: 'fa-face-smile'
                },
                {
                    name: 'Motivation',
                    category: 'Mental & Emotional',
                    description: 'Inspiration, affirmation, or self-talk',
                    icon: 'fa-quote-left'
                },
                {
                    name: 'Note',
                    category: 'Information & Notes',
                    description: 'A textual note or idea',
                    icon: 'fa-sticky-note'
                },
                {
                    name: 'Reference',
                    category: 'Information & Notes',
                    description: 'A saved link, file, or document',
                    icon: 'fa-bookmark'
                },
                {
                    name: 'Journal',
                    category: 'Information & Notes',
                    description: 'A personal reflection or entry',
                    icon: 'fa-pen-fancy'
                },
                {
                    name: 'Transit',
                    category: 'Location-Based',
                    description: 'Movement from one location to another',
                    icon: 'fa-route'
                },
                {
                    name: 'Visit',
                    category: 'Location-Based',
                    description: 'A stop at a specific place',
                    icon: 'fa-map-marker-alt'
                },
                {
                    name: 'Location Alert',
                    category: 'Location-Based',
                    description: 'Triggered on arrival at a specific place',
                    icon: 'fa-bell-on'
                },
                {
                    name: 'Reminder',
                    category: 'Digital & System',
                    description: 'A ping or alert of any kind',
                    icon: 'fa-bell'
                },
                {
                    name: 'Form Submission',
                    category: 'Digital & System',
                    description: 'Time to fill or check a form',
                    icon: 'fa-file-signature'
                },
                {
                    name: 'System Check',
                    category: 'Digital & System',
                    description: 'Scheduled review of automated systems',
                    icon: 'fa-server'
                },
                {
                    name: 'Content Publish',
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
    actions: {
        getIconClass(waypointType){

            const find = this.waypointTypes.find(x => x.name === waypointType);
            if(find?.icon !== undefined) return find.icon;
            else return "fa-square";

        }
    },
})
