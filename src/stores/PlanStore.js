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
            this.plans.push(ref({
                name: 'plan', waypoints: ref([
                    {
                        Type: 'Task',
                        Key: 'MH-123',
                        Color: "White",
                        Source: 'ClickUp',
                        Title: 'Laundry - Adults',
                        Link: '',
                        TargetStart: '2023-09-16T17:00:00.000Z',
                        Spans: [
                            {Type: 'Work', Start: '2023-09-16T17:00:00.000Z', End: '2023-09-16T17:20:00.000Z'}
                        ]
                    },
                    {
                        Type: 'Task',
                        Key: 'MH-124',
                        Source: 'ClickUp',
                        Color: "White",
                        Title: 'Laundry - Kids',
                        Link: '',
                        TargetStart: '2023-09-16T18:00:00.000Z',
                        Spans: [
                            {Type: 'Work', Start: '2023-09-16T18:00:00.000Z', End: '2023-09-16T18:20:00.000Z'}
                        ],
                    },
                    {
                        Type: 'Event',
                        Key: 'CAL: ORTHO',
                        Color: "White",
                        Source: 'O365',
                        Title: "Ortho Appt",
                        Link: '',
                        TargetStart: '2023-09-16T19:00:00.000Z',
                        Spans: [
                            {Type: 'Attend', Start: '2023-09-16T19:00:00.000Z', End: '2023-09-16T19:00:00.000Z'},
                            {Type: 'Transit', Start: '2023-09-16T18:40:00.000Z', End: '2023-09-16T18:55:00.000Z'}
                        ],
                    }
                ])
            }));
        },
    },
})
