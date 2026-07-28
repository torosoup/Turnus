window.ScheduleShiftDetails = (function () {

    async function open(id, week) {

        const response = await fetch(`/Schedule/ShiftDetail?id=${id}&week=${encodeURIComponent(week || '')}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        if (!response.ok) {
            console.error("Failed to load shift details");
            return;
        }

        Modal.renderGlobal(await response.text());
    }

    return {
        open
    };

})();