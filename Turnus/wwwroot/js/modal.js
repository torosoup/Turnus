window.Modal = (function () {

    function renderGlobal(contentHtml) {

        const content = document.getElementById('global-modal-content');
        content.innerHTML = contentHtml;

        const modalElement = document.getElementById('global-modal');

        let modal = bootstrap.Modal.getInstance(modalElement);

        if (!modal) {
            modal = new bootstrap.Modal(modalElement);
        }

        modal.show();
    }

    function close() {

        const modalElement = document.getElementById('global-modal');

        const modal = bootstrap.Modal.getInstance(modalElement);

        if (modal) {
            modal.hide();
        }
    }

    return {
        renderGlobal,
        close
    };

})();