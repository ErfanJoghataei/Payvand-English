// ---------- اسکرول به سکشن ----------
// function scrollToSection(event, sectionId) {
//     const target = document.getElementById(sectionId);

//     if (!target) {
//         console.warn(`Section "${sectionId}" not found`);
//         return;
//     }

//     if (event) event.preventDefault();

//     const yOffset = -20; فاصله از بالا (برای هدر)
//     const start = window.pageYOffset;
//     const end = target.getBoundingClientRect().top + start + yOffset;
//     const distance = end - start;

//     const duration = 800;
//     let startTime = null;

//     function ease(t) {
//         return t < 0.5
//             ? 4 * t * t * t
//             : 1 - Math.pow(-2 * t + 2, 3) / 2;
//     }

//     function animateScroll(timestamp) {
//         if (!startTime) startTime = timestamp;

//         const elapsed = timestamp - startTime;
//         const progress = Math.min(elapsed / duration, 1);
//         const eased = ease(progress);

//         window.scrollTo(0, start + distance * eased);

//         if (elapsed < duration) {
//             requestAnimationFrame(animateScroll);
//         }
//     }

//     requestAnimationFrame(animateScroll);

//     target.classList.add("scroll-highlight");
//     setTimeout(() => target.classList.remove("scroll-highlight"), 1200);
// }

function getScrollOffset() {
    const header = document.querySelector('.header');
    const headerHeight = header ? header.offsetHeight : 0;
    return headerHeight + 24;
}

function syncScrollMargin() {
    const header = document.querySelector('.header');
    const headerHeight = header ? header.offsetHeight : 88;
    document.documentElement.style.setProperty('--header-offset', `${headerHeight}px`);
}

function getScrollTargetTop(target) {
    const scrollRoot = document.scrollingElement || document.documentElement;
    return target.getBoundingClientRect().top + scrollRoot.scrollTop - getScrollOffset();
}

function scrollToSection(event, sectionId) {
    const target = document.getElementById(sectionId);
    if (!target) {
        console.warn(`سکشن با id "${sectionId}" پیدا نشد.`);
        return;
    }

    if (event) {
        event.preventDefault();
    }

    const navMenu = document.querySelector('.nav-menu');
    const menuWasOpen = navMenu?.classList.contains('active');
    if (menuWasOpen) {
        navMenu.classList.remove('active');
    }

    function highlightSection() {
        target.classList.add('scroll-highlight');
        setTimeout(() => target.classList.remove('scroll-highlight'), 1200);
    }

    function scrollWithAnimation() {
        syncScrollMargin();
        const scrollRoot = document.scrollingElement || document.documentElement;
        const start = scrollRoot.scrollTop;
        const end = getScrollTargetTop(target);
        const distance = end - start;
        const duration = 800;
        let startTime = null;

        function ease(t) {
            return t < 0.5
                ? 4 * t * t * t
                : 1 - Math.pow(-2 * t + 2, 3) / 2;
        }

        function animateScroll(timestamp) {
            if (!startTime) startTime = timestamp;
            const elapsed = timestamp - startTime;
            const progress = Math.min(elapsed / duration, 1);

            scrollRoot.scrollTop = start + distance * ease(progress);

            if (elapsed < duration) {
                requestAnimationFrame(animateScroll);
            }
        }

        requestAnimationFrame(animateScroll);
        highlightSection();
    }

    function scrollSmooth() {
        syncScrollMargin();
        // scrollIntoView روی موبایل پایدارتر است؛ آفست با scroll-margin-top در CSS اعمال می‌شود
        target.scrollIntoView({ behavior: 'smooth', block: 'start' });
        highlightSection();
    }

    function performScroll() {
        if (window.innerWidth <= 768) {
            scrollSmooth();
        } else {
            scrollWithAnimation();
        }
    }

    if (menuWasOpen) {
        setTimeout(performScroll, 150);
    } else {
        performScroll();
    }
}
// وقتی صفحه اصلی لود شد بررسی کن آیا از قبل سکشنی ذخیره شده؟
document.addEventListener("DOMContentLoaded", function () {
    syncScrollMargin();
    window.addEventListener('resize', syncScrollMargin);

    const savedSection = sessionStorage.getItem("scrollTarget");
    if (savedSection) {
        sessionStorage.removeItem("scrollTarget");
        setTimeout(() => {
            scrollToSection(null, savedSection);
        }, 200); // کمی تاخیر برای لود کامل DOM
    }

    // ---------- اسکریپت منوی موبایل ----------
    const menuToggle = document.querySelector('.menu-toggle');
    const navMenu = document.querySelector('.nav-menu');

    if (menuToggle && navMenu) {
        menuToggle.addEventListener('click', () => {
            navMenu.classList.toggle('active');
        });
    }

    // اسکرول به سکشن برای لینک‌های داخل منو
    const navLinks = document.querySelectorAll('.nav-menu .nav-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            const section = this.getAttribute('data-section');
            if (section) {
                scrollToSection(e, section);
            }
        });
    });

    // اسکرول به سکشن برای لینک‌های خارج از منو (فوتر)
    document.querySelectorAll('[data-section]').forEach(link => {
        if (!link.closest('.nav-menu')) {
            link.addEventListener('click', function (e) {
                e.preventDefault();
                scrollToSection(e, this.getAttribute('data-section'));
            });
        }
    });

    const qrLoginButton = document.getElementById('qrLoginPromptBtn');
    const qrLoginModal = document.getElementById('qrLoginPromptModal');
    const qrLoginClose = document.getElementById('closeQrLoginPromptModal');
    const qrLoginBack = document.getElementById('qrLoginBackBtn');

    function closeQrLoginModal() {
        if (!qrLoginModal) return;
        qrLoginModal.classList.remove('show');
        qrLoginModal.setAttribute('aria-hidden', 'true');
    }

    function openQrLoginModal() {
        if (!qrLoginModal) return;
        qrLoginModal.classList.add('show');
        qrLoginModal.setAttribute('aria-hidden', 'false');
    }

    qrLoginButton?.addEventListener('click', openQrLoginModal);
    qrLoginClose?.addEventListener('click', closeQrLoginModal);
    qrLoginBack?.addEventListener('click', closeQrLoginModal);

    qrLoginModal?.addEventListener('click', function (event) {
        if (event.target === qrLoginModal) {
            closeQrLoginModal();
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            closeQrLoginModal();
        }
    });
});
