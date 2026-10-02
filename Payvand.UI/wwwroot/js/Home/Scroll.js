//--- اسکرول نرم و تدریجی به سکشن (global) ---
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
