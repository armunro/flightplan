import {defineStore} from 'pinia'
import {ref} from "vue";

export const planStore = defineStore('plan', {
    state: () => {
        return {
            plans: [],
            activePlan: null
        }
    },
    actions: {
        activatePlan(plan) {
            this.activePlan = plan;
        },
        load() {
            this.plans.push({
                    name: 'plan',
                    waypoints: ref([
                        {
                            Type: 'Task',
                            Key: 'TK-001',
                            Color: 'White',
                            Source: 'ClickUp',
                            Title: 'Finalize project proposal',
                            Link: '',
                            TargetStart: '2023-09-16T08:00:00.000Z',
                            Spans: [{ Type: 'Work', Start: '2023-09-16T08:00:00.000Z', End: '2023-09-16T08:45:00.000Z' }]
                        },
                        {
                            Type: 'Focus Session',
                            Key: 'FS-001',
                            Color: 'Cyan',
                            Source: 'Manual',
                            Title: 'Deep work: API architecture',
                            Link: '',
                            TargetStart: '2023-09-16T09:00:00.000Z',
                            Spans: [{ Type: 'Work', Start: '2023-09-16T09:00:00.000Z', End: '2023-09-16T11:00:00.000Z' }]
                        },
                        {
                            Type: 'Break',
                            Key: 'BR-001',
                            Color: 'LightBlue',
                            Source: 'Manual',
                            Title: 'Morning coffee break',
                            Link: '',
                            TargetStart: '2023-09-16T11:00:00.000Z',
                            Spans: [{ Type: 'Rest', Start: '2023-09-16T11:00:00.000Z', End: '2023-09-16T11:15:00.000Z' }]
                        },
                        {
                            Type: 'Meeting',
                            Key: 'MTG-001',
                            Color: 'Yellow',
                            Source: 'Outlook',
                            Title: 'Team stand-up',
                            Link: '',
                            TargetStart: '2023-09-16T11:30:00.000Z',
                            Spans: [{ Type: 'Attend', Start: '2023-09-16T11:30:00.000Z', End: '2023-09-16T12:00:00.000Z' }]
                        },
                        {
                            Type: 'Errand',
                            Key: 'ER-001',
                            Color: 'Orange',
                            Source: 'Manual',
                            Title: 'Grocery shopping',
                            Link: '',
                            TargetStart: '2023-09-16T12:15:00.000Z',
                            Spans: [{ Type: 'Visit', Start: '2023-09-16T12:15:00.000Z', End: '2023-09-16T13:00:00.000Z' }]
                        },
                        {
                            Type: 'Transit',
                            Key: 'TR-001',
                            Color: 'Grey',
                            Source: 'Manual',
                            Title: 'Drive to chiropractor',
                            Link: '',
                            TargetStart: '2023-09-16T13:00:00.000Z',
                            Spans: [{ Type: 'Travel', Start: '2023-09-16T13:00:00.000Z', End: '2023-09-16T13:30:00.000Z' }]
                        },
                        {
                            Type: 'Appointment',
                            Key: 'APPT-001',
                            Color: 'Purple',
                            Source: 'Outlook',
                            Title: 'Chiropractor appointment',
                            Link: '',
                            TargetStart: '2023-09-16T13:30:00.000Z',
                            Spans: [{ Type: 'Attend', Start: '2023-09-16T13:30:00.000Z', End: '2023-09-16T14:00:00.000Z' }]
                        },
                        {
                            Type: 'Phone',
                            Key: 'PHN-001',
                            Color: 'Pink',
                            Source: 'Manual',
                            Title: 'Call Mom',
                            Link: '',
                            TargetStart: '2023-09-16T14:15:00.000Z',
                            Spans: [{ Type: 'Call', Start: '2023-09-16T14:15:00.000Z', End: '2023-09-16T14:30:00.000Z' }]
                        },
                        {
                            Type: 'Journal',
                            Key: 'JRNL-001',
                            Color: 'Gold',
                            Source: 'Manual',
                            Title: 'Reflect on morning progress',
                            Link: '',
                            TargetStart: '2023-09-16T14:45:00.000Z',
                            Spans: [{ Type: 'Write', Start: '2023-09-16T14:45:00.000Z', End: '2023-09-16T15:00:00.000Z' }]
                        },
                        {
                            Type: 'Review',
                            Key: 'RVW-001',
                            Color: 'Teal',
                            Source: 'ClickUp',
                            Title: 'Review pull requests',
                            Link: '',
                            TargetStart: '2023-09-16T15:00:00.000Z',
                            Spans: [{ Type: 'Review', Start: '2023-09-16T15:00:00.000Z', End: '2023-09-16T15:45:00.000Z' }]
                        },
                        {
                            Type: 'Email',
                            Key: 'EML-001',
                            Color: 'LightGreen',
                            Source: 'Outlook',
                            Title: 'Respond to client questions',
                            Link: '',
                            TargetStart: '2023-09-16T16:00:00.000Z',
                            Spans: [{ Type: 'Write', Start: '2023-09-16T16:00:00.000Z', End: '2023-09-16T16:30:00.000Z' }]
                        },
                        {
                            Type: 'Checkpoint',
                            Key: 'CHK-001',
                            Color: 'Blue',
                            Source: 'ClickUp',
                            Title: 'Mid-project checkpoint meeting',
                            Link: '',
                            TargetStart: '2023-09-16T16:45:00.000Z',
                            Spans: [{ Type: 'Attend', Start: '2023-09-16T16:45:00.000Z', End: '2023-09-16T17:30:00.000Z' }]
                        },
                        {
                            Type: 'Brainstorm',
                            Key: 'BRN-001',
                            Color: 'Indigo',
                            Source: 'Manual',
                            Title: 'Ideas for Q4 Initiatives',
                            Link: '',
                            TargetStart: '2023-09-16T17:45:00.000Z',
                            Spans: [{ Type: 'Think', Start: '2023-09-16T17:45:00.000Z', End: '2023-09-16T18:30:00.000Z' }]
                        },
                        {
                            Type: 'Break',
                            Key: 'DIN-001',
                            Color: 'Brown',
                            Source: 'Manual',
                            Title: 'Family Dinner',
                            Link: '',
                            TargetStart: '2023-09-16T18:45:00.000Z',
                            Spans: [{ Type: 'Eat', Start: '2023-09-16T18:45:00.000Z', End: '2023-09-16T19:30:00.000Z' }]
                        },
                        {
                            Type: 'Content Publish',
                            Key: 'PBL-001',
                            Color: 'Magenta',
                            Source: 'ClickUp',
                            Title: 'Publish weekly blog post',
                            Link: '',
                            TargetStart: '2023-09-16T20:00:00.000Z',
                            Spans: [{ Type: 'Post', Start: '2023-09-16T20:00:00.000Z', End: '2023-09-16T20:30:00.000Z' }]
                        },
                        {
                            Type: 'Mood Check',
                            Key: 'MOOD-001',
                            Color: 'Peach',
                            Source: 'Manual',
                            Title: 'Evening mood check-in',
                            Link: '',
                            TargetStart: '2023-09-16T21:00:00.000Z',
                            Spans: [{ Type: 'Reflect', Start: '2023-09-16T21:00:00.000Z', End: '2023-09-16T21:15:00.000Z' }]
                        }
                    ])
                }
            );
        },
    },
})
