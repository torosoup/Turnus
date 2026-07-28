window.DashboardUserManagement = (function () {

    async function openManageModal(id) {
        const response = await fetch(`/Users/Manage?id=${id}`, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        const html = await response.text();
        Modal.renderGlobal(html);
        hookForm('#user-manage-form');
    }

    async function openInspectModal(id) {
        const response = await fetch(`/Users/Details?id=${id}`, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        const html = await response.text();
        Modal.renderGlobal(html);
    }

    async function openDeleteModal(id) {
        const response = await fetch(`/Users/Delete?id=${id}`, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        const html = await response.text();
        Modal.renderGlobal(html);
        hookForm('#user-delete-form');
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

    return { openManageModal, openInspectModal, openDeleteModal };

})();
