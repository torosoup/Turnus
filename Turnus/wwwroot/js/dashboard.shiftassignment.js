window.DashboardShiftAssignment = (function () {

    async function openAssignModal(
        scheduledShiftId,
        roleId,
        venueId,
        date,
        employeeId = null) {

        let url =
            `/ShiftAssignment/Create?scheduledShiftId=${scheduledShiftId}`
            + `&roleId=${roleId}`
            + `&venueId=${venueId}`
            + `&date=${date}`;

        if (employeeId)
            url += `&employeeId=${employeeId}`;

        const response = await fetch(url, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        Modal.renderGlobal(await response.text());

        hookForm("#assignshift-form");
    }

    async function openUnassignModal(
        assignmentId,
        venueId,
        date) {

        const response = await fetch(
            `/ShiftAssignment/Delete?id=${assignmentId}&venueId=${venueId}&date=${date}`,
            {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

        Modal.renderGlobal(await response.text());

        hookForm("#unassignshift-form");
    }

    function hookForm(selector) {

        const form = document.querySelector(selector);

        if (!form)
            return;

        form.addEventListener("submit", async function (e) {

            e.preventDefault();

            const formData = new FormData(form);

            const response = await fetch(form.action, {
                method: form.method,
                body: formData
            });

            if (response.redirected)
                window.location.href = response.url;
            else
                Modal.renderGlobal(await response.text());

        });
    }

    return {
        openAssignModal,
        openUnassignModal
    };

})(); 