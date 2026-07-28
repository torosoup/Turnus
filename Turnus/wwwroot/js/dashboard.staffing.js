window.DashboardStaffing = (function () {

    async function openCreateModal(departmentId) {
        const response = await fetch(`/StaffingRequirements/Create?departmentId=${departmentId}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        const html = await response.text();
        Modal.renderGlobal(html);
        hookForm('#staffing-create-form');
        setupShiftScopedCheckbox();
    }

    async function openEditModal(id) {
        const response = await fetch(`/StaffingRequirements/Edit?id=${id}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        const html = await response.text();
        Modal.renderGlobal(html);
        hookForm('#staffing-edit-form');
        setupShiftScopedCheckbox();
    }

    async function openDeleteModal(id) {
        const response = await fetch(`/StaffingRequirements/Delete?id=${id}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        const html = await response.text();
        Modal.renderGlobal(html);
        hookForm('#staffing-delete-form');
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
            }
            else {
                const html = await response.text();
                Modal.renderGlobal(html);
            }
        });
    }

    function setupShiftScopedCheckbox() {
        const checkbox = document.getElementById("not-shift-scoped");
        const hiddenField = document.getElementById("is-shift-scoped");

        if (!checkbox || !hiddenField) return;

        checkbox.addEventListener("change", function () {
            hiddenField.value = !this.checked;
        });
    }

    return {
        openCreateModal,
        openEditModal,
        openDeleteModal
    };

})();   