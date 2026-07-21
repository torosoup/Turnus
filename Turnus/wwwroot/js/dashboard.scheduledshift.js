window.DashboardScheduledShift = (function () {

    async function openCreateModal(venueId, departmentId, date,) {
        const response = await fetch(`/ScheduledShifts/Create?venueId=${venueId}&departmentId=${departmentId}&date=${date}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        Modal.open(await response.text());
        hookForm('#scheduledshift-create-form');
    }

    async function openDeleteModal(id) {
        const response = await fetch(`/ScheduledShifts/Delete/${id}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        Modal.open(await response.text());
        hookForm('#scheduledshift-delete-form');
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
        openCreateModal,
        openDeleteModal
    };
})();
