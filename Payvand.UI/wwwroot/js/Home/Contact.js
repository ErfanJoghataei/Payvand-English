document.addEventListener('DOMContentLoaded', function () {

    // ===== Modal =====
    const openModalBtn = document.getElementById('ContactBtn');
    const closeModalBtn = document.getElementById('closeModal');
    const contactModal = document.getElementById('contactModal');
    const openViolationReportBtn = document.getElementById('ViolationReportBtn');
    const closeViolationReportBtn = document.getElementById('closeViolationReportModal');
    const violationReportModal = document.getElementById('violationReportModal');

    function openModal(modal) {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeModal(modal) {
        modal.classList.remove('active');
        document.body.style.overflow = 'auto';
    }

    if (openModalBtn && closeModalBtn && contactModal) {

        openModalBtn.addEventListener('click', function (e) {
            e.preventDefault();
            openModal(contactModal);
        });

        closeModalBtn.addEventListener('click', function () {
            closeModal(contactModal);
        });

        contactModal.addEventListener('click', function (e) {
            if (e.target === contactModal) {
                closeModal(contactModal);
            }
        });
    }

    if (openViolationReportBtn && closeViolationReportBtn && violationReportModal) {
        openViolationReportBtn.addEventListener('click', function (e) {
            e.preventDefault();
            openModal(violationReportModal);
        });

        closeViolationReportBtn.addEventListener('click', function () {
            closeModal(violationReportModal);
        });

        violationReportModal.addEventListener('click', function (e) {
            if (e.target === violationReportModal) {
                closeModal(violationReportModal);
            }
        });

        if (window.location.hash === '#violation-report') {
            openModal(violationReportModal);
        }
    }

});

document.addEventListener('DOMContentLoaded', function () {
    const featuresSection = document.getElementById('features');


    if (featuresSection) {
        const accordionItems = featuresSection.querySelectorAll('.accordion-item');
        let activeItem = featuresSection.querySelector('.accordion-item.active');
        function initAccordion() {
            // Set height for active item
            if (activeItem) {
                const content = activeItem.querySelector('.accordion-content');
                const body = activeItem.querySelector('.accordion-body');
                content.style.height = body.scrollHeight + 'px';
            }

            // Add click event listeners
            accordionItems.forEach(item => {
                const header = item.querySelector('.accordion-header');
                header.addEventListener('click', () => toggleAccordion(item));
            });
        }

        function toggleAccordion(item) {
            if (item === activeItem) {
                const activeContent = activeItem.querySelector('.accordion-content');
                activeItem.classList.remove('active');
                activeContent.style.height = '0px';
                activeItem = null;
                return;
            }

            // Close currently active item
            if (activeItem) {
                const activeContent = activeItem.querySelector('.accordion-content');
                activeItem.classList.remove('active');
                activeContent.style.height = '0px';
            }

            // Open clicked item
            const content = item.querySelector('.accordion-content');
            const body = item.querySelector('.accordion-body');

            item.classList.add('active');
            content.style.height = body.scrollHeight + 'px';

            activeItem = item;
        }

        // Initialize on load
        initAccordion();

        // Handle window resize
        let resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                if (activeItem) {
                    const activeContent = activeItem.querySelector('.accordion-content');
                    const activeBody = activeItem.querySelector('.accordion-body');
                    activeContent.style.height = activeBody.scrollHeight + 'px';
                }
            }, 250);
        });
    }


});

document.addEventListener('DOMContentLoaded', function () {
    const faqSection = document.getElementById('faq');


    if (faqSection) {
        const accordionItems = faqSection.querySelectorAll('.accordion-item');
        let activeItem = faqSection.querySelector('.accordion-item.active');
        function initAccordion() {
            if (activeItem) {
                const content = activeItem.querySelector('.accordion-content');
                const body = activeItem.querySelector('.accordion-body');
                content.style.height = body.scrollHeight + 'px';
            }

            accordionItems.forEach(item => {
                const header = item.querySelector('.accordion-header');
                header.addEventListener('click', () => toggleAccordion(item));
            });
        }

        function toggleAccordion(item) {
            if (item === activeItem) {
                const activeContent = activeItem.querySelector('.accordion-content');
                activeItem.classList.remove('active');
                activeContent.style.height = '0px';
                activeItem = null;
                return;
            }

            if (activeItem) {
                const activeContent = activeItem.querySelector('.accordion-content');
                activeItem.classList.remove('active');
                activeContent.style.height = '0px';
            }

            const content = item.querySelector('.accordion-content');
            const body = item.querySelector('.accordion-body');

            item.classList.add('active');
            content.style.height = body.scrollHeight + 'px';

            activeItem = item;
        }

        initAccordion();

        let resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                if (activeItem) {
                    const activeContent = activeItem.querySelector('.accordion-content');
                    const activeBody = activeItem.querySelector('.accordion-body');
                    activeContent.style.height = activeBody.scrollHeight + 'px';
                }
            }, 250);
        });
    }
 
});
