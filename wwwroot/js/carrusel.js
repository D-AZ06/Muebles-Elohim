(function () {
    const INTERVALO = 4000; // debe coincidir con animation-duration en Home.razor.css
    let temporizador = null;

    function indiceActual(c) {
        return [...c.querySelectorAll('.slide-item')].findIndex(s => s.classList.contains('active'));
    }

    function mostrar(c, i) {
        const slides = c.querySelectorAll('.slide-item');
        const dots = c.querySelectorAll('.dot-item');
        if (!slides.length) return;

        const n = (i + slides.length) % slides.length;

        slides.forEach((s, k) => s.classList.toggle('active', k === n));

        // Se quita y se vuelve a poner "active" para reiniciar la barra de progreso
        dots.forEach(d => d.classList.remove('active'));
        void c.offsetWidth;
        if (dots[n]) dots[n].classList.add('active');
    }

    function reiniciarTemporizador(c) {
        clearInterval(temporizador);
        temporizador = setInterval(() => {
            if (!document.body.contains(c)) {
                clearInterval(temporizador);
                return;
            }
            mostrar(c, indiceActual(c) + 1);
        }, INTERVALO);
    }

    function iniciar() {
        const c = document.querySelector('.carousel-container');
        if (!c || c._listo) return;
        c._listo = true;

        c.addEventListener('click', e => {
            const dot = e.target.closest('.dot-item');
            if (!dot) return;

            const n = [...c.querySelectorAll('.dot-item')].indexOf(dot);
            if (n >= 0 && n !== indiceActual(c)) {
                mostrar(c, n);
                reiniciarTemporizador(c);
            }
        });

        mostrar(c, 0);
        reiniciarTemporizador(c);
    }

    new MutationObserver(iniciar).observe(document.body, { childList: true, subtree: true });
    iniciar();
})();