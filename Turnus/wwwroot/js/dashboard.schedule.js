window.DashboardSchedule = (function () {
    function filterByDate(date) {
        // simplest version: reload dashboard with query param
        const url = `/Admin/Dashboard?date=${date}`;
        window.location.href = url;
    }

    function openCreateShiftModal(date) {
        // you’ll add a ScheduleController endpoint later
        alert(`Create shift for ${date} (implement modal + endpoint)`);
    }

    function openDeleteShiftModal(id) {
        alert(`Delete shift ${id} (implement modal + endpoint)`);
    }

    function openReview(venueId, date) {
        window.location.href = `/ScheduleReview/Review?venueId=${venueId}&date=${date}`;
    }

    return { filterByDate, openCreateShiftModal, openDeleteShiftModal, openReview };
})();
