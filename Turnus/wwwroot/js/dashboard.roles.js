window.DashboardRoles = (function () {

    async function openCreateModal() {
        const response = await fetch('/Roles/Create', {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        const html = await response.text();
        Modal.renderGlobal(html);

        hookForm('#role-create-form');
        setupScopeSelection();
    }


    async function openEditModal(id) {
        const response = await fetch(`/Roles/Edit/${id}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        const html = await response.text();
        Modal.renderGlobal(html);

        hookForm('#role-edit-form');
        setupScopeSelection();
    }


    async function openDeleteModal(id) {
        const response = await fetch(`/Roles/Delete/${id}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        const html = await response.text();
        Modal.renderGlobal(html);

        hookForm('#role-delete-form');
    }


    function setupScopeSelection() {
        const scopeSelect = document.getElementById("role-scope");
        if (!scopeSelect) return;

        const venueContainer = document.getElementById("venue-selection");
        const departmentContainer = document.getElementById("department-selection");

        const venueSelect = venueContainer.querySelector("select");
        const departmentSelect = departmentContainer.querySelector("select");

        function updateScope() {
            const scope = scopeSelect.value;

            // Hide and disable both
            venueContainer.classList.add("d-none");
            departmentContainer.classList.add("d-none");

            venueSelect.disabled = true;
            departmentSelect.disabled = true;

            if (scope === "Venue") {
                venueContainer.classList.remove("d-none");
                venueSelect.disabled = false;

                // Clear the inactive one
                departmentSelect.value = "";
            }
            else if (scope === "Department") {
                departmentContainer.classList.remove("d-none");
                departmentSelect.disabled = false;

                // Clear the inactive one
                venueSelect.value = "";
            }
            else {
                venueSelect.value = "";
                departmentSelect.value = "";
            }
        }

        scopeSelect.addEventListener("change", updateScope);
        updateScope();
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

                // Restore scope behavior after validation errors
                setupScopeSelection();
            }
        });
    }


    return {
        openCreateModal,
        openEditModal,
        openDeleteModal
    };

})();