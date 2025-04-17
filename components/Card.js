export default {
    props: {
        data: {
            type: Object,
            required: true
        },
    },
    methods:{
        formatDateRange(startDate, endDate) {
            // Parse the start and end date strings into Date objects
            const start = new Date(startDate);
            const end = new Date(endDate);

            // Helper function to format the date to "YYYY-MM-DD" and time to "HH:mm"
            const formatDate = (date) => {
                const year = date.getUTCFullYear();
                const month = String(date.getUTCMonth() + 1).padStart(2, '0');
                const day = String(date.getUTCDate()).padStart(2, '0');
                const hours = String(date.getUTCHours()).padStart(2, '0');
                const minutes = String(date.getUTCMinutes()).padStart(2, '0');

                return { date: `${year}-${month}-${day}`, time: `${hours}:${minutes}` };
            };

            // Format the start and end dates
            const formattedStart = formatDate(start);
            const formattedEnd = formatDate(end);

            // If the start and end dates are the same, return only one date with the times
            if (formattedStart.date === formattedEnd.date) {
                return `${formattedStart.date} ${formattedStart.time} - ${formattedEnd.time}`;
            }

            // Otherwise, return both start and end dates with times
            return `${formattedStart.date} ${formattedStart.time} - ${formattedEnd.date} ${formattedEnd.time}`;
    }
    },
    template: `

    <div class="task-card">
          <span class="fp-ref">{{ data.Key }}</span> FROM <span class="fp-source">{{data.Source }}</span><br>
          <div v-for="(span, index) in data.Spans">
            <span class="fp-label">{{span.Type}}</span><br>
            <span class="fp-value">{{ formatDateRange(span.Start, span.End) }}</span>
          </div>
                   
    </div>
  `
}
