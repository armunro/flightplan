export default {
    formatDate(inputDate) {
        // Convert inputDate to a Date object
        const date = new Date(inputDate);

        // Check if the date is valid
        if (isNaN(date.getTime())) {
            return "Invalid Date";
        }

        // Get the month, day, and year from the date object
        const month = date.getMonth() + 1;
        const day = date.getDate();
        const year = date.getFullYear().toString().slice(-2);

        // Return the date in "MM/DD/YY" format
        return `${month.toString().padStart(2, "0")}/${day.toString().padStart(2, "0")}/${year}`;
    },
    
    
    formatTime(i) {
        if (i < 10) {
            i = "0" + i;
        }
        return i;
    },

    formatCountdown(targetDate) {
        let distance = (new Date(targetDate).getTime()) - (new Date().getTime());
        let days = Math.floor(distance / (1000 * 60 * 60 * 24));
        let hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        let minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        let seconds = Math.floor((distance % (1000 * 60)) / 1000);
        return (days <= 0 ? "" : days + "d ") +
            (hours <= 0 ? "" : hours + "h ") +
            (minutes <= 0 ? "" : this.formatTime(minutes) + "m ") +
            this.formatTime(seconds) + "s ";
    },


}