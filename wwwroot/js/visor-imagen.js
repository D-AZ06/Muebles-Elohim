// Visor de imagen a pantalla completa con zoom: rueda, pellizco, doble clic/toque, botones +/-, arrastrar.
(function () {
    const MIN = 1, MAX = 5;
    let abierto = false;

    function abrir(src, alt) {
        if (abierto) return;
        abierto = true;

        const previo = document.activeElement;
        const visor = document.createElement('div');
        visor.className = 'visor-img';
        visor.setAttribute('role', 'dialog');
        visor.setAttribute('aria-modal', 'true');
        visor.setAttribute('aria-label', 'Imagen ampliada');
        visor.innerHTML =
            '<div class="visor-stage"><img class="visor-foto" draggable="false" alt=""></div>' +
            '<div class="visor-barra">' +
            '<button type="button" data-acc="menos" aria-label="Alejar"><i class="bi bi-dash-lg"></i></button>' +
            '<button type="button" data-acc="mas" aria-label="Acercar"><i class="bi bi-plus-lg"></i></button>' +
            '<button type="button" data-acc="reset" aria-label="Restablecer"><i class="bi bi-arrows-angle-contract"></i></button>' +
            '</div>' +
            '<button type="button" class="visor-cerrar" data-acc="cerrar" aria-label="Cerrar"><i class="bi bi-x-lg"></i></button>';

        const stage = visor.querySelector('.visor-stage');
        const img = visor.querySelector('.visor-foto');
        img.src = src;
        img.alt = alt || '';

        let s = 1, x = 0, y = 0, mov = 0, tap = 0, pinch = null;
        const punteros = new Map();

        function aplicar() {
            const mx = Math.max(0, (img.offsetWidth * s - stage.clientWidth) / 2);
            const my = Math.max(0, (img.offsetHeight * s - stage.clientHeight) / 2);
            x = Math.min(mx, Math.max(-mx, x));
            y = Math.min(my, Math.max(-my, y));
            img.style.transform = 'translate(' + x + 'px,' + y + 'px) scale(' + s + ')';
            stage.classList.toggle('con-zoom', s > 1.01);
        }

        // cx, cy: punto de zoom relativo al centro del visor
        function zoomEn(ns, cx, cy) {
            ns = Math.min(MAX, Math.max(MIN, ns));
            const k = ns / s;
            x = cx - (cx - x) * k;
            y = cy - (cy - y) * k;
            s = ns;
            if (s === 1) { x = 0; y = 0; }
            aplicar();
        }

        function centro(px, py) {
            const r = stage.getBoundingClientRect();
            return [px - r.left - r.width / 2, py - r.top - r.height / 2];
        }

        function cerrar() {
            document.removeEventListener('keydown', tecla, true);
            window.removeEventListener('resize', aplicar);
            visor.remove();
            abierto = false;
            if (previo && previo.focus) previo.focus();
        }

        function tecla(e) {
            if (e.key === 'Escape') { e.preventDefault(); e.stopPropagation(); cerrar(); }
            else if (e.key === '+' || e.key === '=') zoomEn(s * 1.5, 0, 0);
            else if (e.key === '-') zoomEn(s / 1.5, 0, 0);
        }

        stage.addEventListener('wheel', function (e) {
            e.preventDefault();
            const c = centro(e.clientX, e.clientY);
            zoomEn(s * (e.deltaY < 0 ? 1.2 : 1 / 1.2), c[0], c[1]);
        }, { passive: false });

        stage.addEventListener('pointerdown', function (e) {
            stage.setPointerCapture(e.pointerId);
            punteros.set(e.pointerId, { x: e.clientX, y: e.clientY });
            if (punteros.size === 1) mov = 0;
            if (punteros.size === 2) {
                const p = Array.from(punteros.values());
                pinch = { d: Math.hypot(p[0].x - p[1].x, p[0].y - p[1].y), s: s };
            }
        });

        stage.addEventListener('pointermove', function (e) {
            const p = punteros.get(e.pointerId);
            if (!p) return;
            const dx = e.clientX - p.x, dy = e.clientY - p.y;
            p.x = e.clientX; p.y = e.clientY;

            if (punteros.size === 2 && pinch) {
                const v = Array.from(punteros.values());
                const d = Math.hypot(v[0].x - v[1].x, v[0].y - v[1].y);
                const c = centro((v[0].x + v[1].x) / 2, (v[0].y + v[1].y) / 2);
                mov = 99;
                zoomEn(pinch.s * d / pinch.d, c[0], c[1]);
            } else if (punteros.size === 1) {
                mov += Math.abs(dx) + Math.abs(dy);
                if (s > 1) { x += dx; y += dy; aplicar(); }
            }
        });

        function soltar(e) {
            if (!punteros.has(e.pointerId)) return;
            punteros.delete(e.pointerId);
            if (punteros.size < 2) pinch = null;

            // doble clic / doble toque: alterna entre ajustado y 2.5x
            if (e.type === 'pointerup' && punteros.size === 0 && mov < 8) {
                const t = Date.now();
                if (t - tap < 300) {
                    const c = centro(e.clientX, e.clientY);
                    zoomEn(s > 1.01 ? 1 : 2.5, c[0], c[1]);
                    tap = 0;
                } else {
                    tap = t;
                }
            }
        }
        stage.addEventListener('pointerup', soltar);
        stage.addEventListener('pointercancel', soltar);

        visor.addEventListener('click', function (e) {
            const b = e.target.closest('[data-acc]');
            if (!b) return;
            const a = b.dataset.acc;
            if (a === 'cerrar') cerrar();
            else if (a === 'mas') zoomEn(s * 1.5, 0, 0);
            else if (a === 'menos') zoomEn(s / 1.5, 0, 0);
            else zoomEn(1, 0, 0);
        });

        document.addEventListener('keydown', tecla, true);
        window.addEventListener('resize', aplicar);
        document.body.appendChild(visor);
        visor.querySelector('.visor-cerrar').focus();
    }

    // Delegado: funciona con lo que Blazor dibuja después y sin onclick en línea (la CSP los bloquea)
    function disparador(e) {
        return e.target.closest ? e.target.closest('[data-visor]') : null;
    }

    document.addEventListener('click', function (e) {
        const el = disparador(e);
        if (!el) return;
        e.preventDefault();
        abrir(el.dataset.foto, el.dataset.titulo);
    });

    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter' && e.key !== ' ') return;
        const el = disparador(e);
        if (!el || el.tagName === 'BUTTON') return;
        e.preventDefault();
        abrir(el.dataset.foto, el.dataset.titulo);
    });

    window.visorImagen = { abrir: abrir };
})();