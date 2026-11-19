(function () {
    function initFloatingBtn() {
        const sentinel = document.getElementById('floatingSentinel');
        const btn = document.querySelector('.floating-btn-container');
        if (!sentinel || !btn || sentinel._observed) return;
        sentinel._observed = true;

        new IntersectionObserver(([entry]) => {
            btn.classList.toggle('at-bottom', entry.isIntersecting);
        }, { rootMargin: '0px 0px -30px 0px' }).observe(sentinel);
    }

    new MutationObserver(initFloatingBtn).observe(document.body, { childList: true, subtree: true });
    initFloatingBtn();
})();