(function () {
    let ultimoY = window.scrollY;
    let pendiente = false;

    function revisar() {
        pendiente = false;
        const sentinel = document.getElementById('floatingSentinel');
        const btn = document.querySelector('.floating-btn-container');
        if (!sentinel || !btn) return;

        const y = window.scrollY;
        const dist = sentinel.getBoundingClientRect().top - window.innerHeight;

        if (dist > 40) {
            btn.classList.remove('at-bottom');
        } else if (y > ultimoY) {
            btn.classList.add('at-bottom');          // bajando: expande 40px antes
        } else if (y < ultimoY) {
            if (dist > -26) btn.classList.remove('at-bottom');  // subiendo: colapsa
        } else {
            btn.classList.toggle('at-bottom', dist <= 0);
        }
        ultimoY = y;
    }

    function pedirRevision() {
        if (pendiente) return;
        pendiente = true;
        requestAnimationFrame(revisar);
    }

    window.addEventListener('scroll', pedirRevision, { passive: true });
    window.addEventListener('resize', pedirRevision);
    new MutationObserver(pedirRevision).observe(document.body, { childList: true, subtree: true });
    pedirRevision();
})();