window.DashboardAssignments = (function () {

    async function openAssignModal(scheduledShiftId) {
        const response = await fetch(`/ScheduleReview/AssignShift?scheduledShiftId=${scheduledShiftId}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        Modal.open(await response.text());
        hookForm('#assignshift-form');
    }

    async function openUnassignModal(shiftAssignmentId, venueId, date) {
        const response = await fetch(`/ScheduleReview/UnassignShift?shiftAssignmentId=${shiftAssignmentId}&venueId=${venueId}&date=${date}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        Modal.open(await response.text());
        hookForm('#unassignshift-form');
    }

    function hookForm(selector) {
        const form = document.querySelector(selector);
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            const formData = new FormData(form);
            const response = await fetch(form.action, {
                method: form.method,
                body: formData
            });

            if (response.redirected) {
                window.location.href = response.url;
            } else {
                Modal.open(await response.text());
            }
        });
    }

    return {
        openAssignModal,
        openUnassignModal
    };
})();
