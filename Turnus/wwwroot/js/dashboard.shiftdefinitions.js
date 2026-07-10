window.DashboardShiftDefinitions = (function () {

    async function openCreateModal(departmentId) {
        const response = await fetch(`/ShiftDefinitions/Create?departmentId=${departmentId}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const html = await response.text();
        Modal.open(html);
        hookForm('#shiftdefinition-create-form');
    }

    async function openEditModal(id) {
        const response = await fetch(`/ShiftDefinitions/Edit?id=${id}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const html = await response.text();
        Modal.open(html);
        hookForm('#shiftdefinition-edit-form');
    }

    async function openDeleteModal(id) {
        const response = await fetch(`/ShiftDefinitions/Delete?id=${id}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const html = await response.text();
        Modal.open(html);
        hookForm('#shiftdefinition-delete-form');
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
                const html = await response.text();
                Modal.open(html);
            }
        });
    }

    return { openCreateModal, openEditModal, openDeleteModal };

})();