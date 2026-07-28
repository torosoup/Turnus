window.DashboardDepartments = (function () {
    async function openCreateModal(venueId) {
        const response = await fetch(`/Departments/Create?venueId=${venueId}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const html = await response.text();
        Modal.renderGlobal(html);
        hookForm('#department-create-form');
    }

    async function openEditModal(id) {
        const response = await fetch(`/Departments/Edit/${id}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const html = await response.text();
        Modal.renderGlobal(html);
        hookForm('#department-edit-form');
    }

    async function openDeleteModal(id) {
        const response = await fetch(`/Departments/Delete/${id}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const html = await response.text();
        Modal.renderGlobal(html);
        hookForm('#department-delete-form');
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
                Modal.renderGlobal(html);
            }
        });
    }

    return { openCreateModal, openEditModal, openDeleteModal };
})();
