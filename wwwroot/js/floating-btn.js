(function () {
    const root = document.documentElement;

    function updateFooterLift() {
        const footer = document.querySelector('footer.footer');
        let lift = 0;

        if (footer) {
            const overlap = root.clientHeight - footer.getBoundingClientRect().top;
            lift = Math.max(0, Math.round(overlap));
        }

        root.style.setProperty('--footer-lift', lift + 'px');
    }

    let pendiente = false;
    function updateFooterLiftSoon() {
        if (pendiente) return;
        pendiente = true;
        requestAnimationFrame(() => {
            pendiente = false;
            updateFooterLift();
        });
    }

    function initFloatingBtn() {
        const sentinel = document.getElementById('floatingSentinel');
        const btn = document.querySelector('.floating-btn-container');
        if (!sentinel || !btn || sentinel._observed) return;
        sentinel._observed = true;

        new IntersectionObserver(([entry]) => {
            btn.classList.toggle('at-bottom', entry.isIntersecting);
        }, { rootMargin: '0px 0px -30px 0px' }).observe(sentinel);
    }

    window.addEventListener('scroll', updateFooterLift, { passive: true });
    window.addEventListener('resize', updateFooterLiftSoon);

    if ('ResizeObserver' in window) {
        new ResizeObserver(updateFooterLiftSoon).observe(document.body);
    }

    new MutationObserver(() => {
        initFloatingBtn();
        updateFooterLiftSoon();
    }).observe(document.body, { childList: true, subtree: true });

    initFloatingBtn();
    updateFooterLift();
})();