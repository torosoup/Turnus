window.DashboardScheduleReview = (function () {

    async function openReviewModal(venueId, date) {

        const response = await fetch(
            `/ScheduleReview/Review?venueId=${venueId}&date=${date}`,
            {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            }
        );

        const html = await response.text();

        Modal.open(html);

        hookForms();
    }


    function hookForms() {

        const container = document.querySelector(
            "#schedule-review-modal"
        );

        if (!container)
            return;


        container.querySelectorAll("form")
            .forEach(form => {

                form.addEventListener("submit", async function (e) {

                    e.preventDefault();


                    const response = await fetch(
                        form.action,
                        {
                            method: form.method,
                            body: new FormData(form)
                        }
                    );


                    const html = await response.text();


                    Modal.open(html);


                    hookForms();

                });

            });

    }


    return {
        openReviewModal
    };

})();