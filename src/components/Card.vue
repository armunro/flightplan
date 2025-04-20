<template>
  <div class="task-card">
    <span class="fp-ref">{{ data.Key }}</span> FROM <span class="fp-source">{{ data.Source }}</span><br>
    <div v-for="(span, index) in data.Spans" :key="index">
      <span class="fp-label">{{ span.Type }}</span><br>
      <span class="fp-value">{{ formatDateRange(span.Start, span.End) }}</span>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Card',
  props: {
    data: {
      type: Object,
      required: true
    }
  },
  methods: {
    formatDateRange(startDate, endDate) {
      const start = new Date(startDate);
      const end = new Date(endDate);

      const formatDate = (date) => {
        const year = date.getUTCFullYear();
        const month = String(date.getUTCMonth() + 1).padStart(2, '0');
        const day = String(date.getUTCDate()).padStart(2, '0');
        const hours = String(date.getUTCHours()).padStart(2, '0');
        const minutes = String(date.getUTCMinutes()).padStart(2, '0');
        return { date: `${year}-${month}-${day}`, time: `${hours}:${minutes}` };
      };

      const formattedStart = formatDate(start);
      const formattedEnd = formatDate(end);

      if (formattedStart.date === formattedEnd.date) {
        return `${formattedStart.date} ${formattedStart.time} - ${formattedEnd.time}`;
      }

      return `${formattedStart.date} ${formattedStart.time} - ${formattedEnd.date} ${formattedEnd.time}`;
    }
  }
}
</script>

<style scoped>
/* Add styles if needed */
</style>
