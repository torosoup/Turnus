window.DashboardVenues = (function () {
    async function openCreateModal() {
        const response = await fetch('/Venues/Create', { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        const html = await response.text();
        Modal.open(html);
        hookForm('#venue-create-form');
    }

    async function openEditModal(id) {
        const response = await fetch(`/Venues/Edit/${id}`, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        const html = await response.text();
        Modal.open(html);
        hookForm('#venue-edit-form');
    }

    async function openDeleteModal(id) {
        const response = await fetch(`/Venues/Delete/${id}`, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        const html = await response.text();
        Modal.open(html);
        hookForm('#venue-delete-form');
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
                Modal.open(html); // show validation errors if any
            }
        });
    }

    return { openCreateModal, openEditModal, openDeleteModal };
})();
