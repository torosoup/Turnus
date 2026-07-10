window.Modal = (function () {
    function open(contentHtml) {
        const container = document.getElementById('dashboard-modal-container');
        container.innerHTML = `
            <div class="modal fade show" style="display:block;" tabindex="-1">
              <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                  ${contentHtml}
                </div>
              </div>
            </div>
            <div class="modal-backdrop fade show"></div>`;
    }

    function close() {
        const container = document.getElementById('dashboard-modal-container');
        container.innerHTML = '';
    }

    return { open, close };
})();
