(function () {
    const GAP = 20; // separación fija entre el botón y el footer

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

    window.addEventListener('scroll', update, { passive: true });
    window.addEventListener('resize', update);

    // Blazor pinta la vista después de cargar el script y al navegar
    new MutationObserver(update).observe(document.body, { childList: true, subtree: true });
    update();
})();