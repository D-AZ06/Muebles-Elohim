// Altura del header grande (el que se esconde al hacer scroll)
function alturaHeader() {
    const header = document.getElementById('mainHeader');
    return header ? header.offsetHeight : 0;
}

// Deja la pagina con el header grande escondido y la barra sticky arriba.
// Si el usuario hace scroll hacia arriba, el header se muestra normal.
window.colapsarHeader = function () {
    window.__scrollLista = null;
    window.__fondoY = null;
    window.scrollTo(0, alturaHeader());
};

function colapsarAlCargar() {
    // Respeta la posicion si el navegador ya restauro un scroll mas abajo
    if (window.scrollY < alturaHeader()) {
        window.colapsarHeader();
    }
}

colapsarAlCargar();
window.addEventListener('load', colapsarAlCargar);

// --- Enlace a la misma pagina donde ya estas: Blazor sube al inicio, se vuelve a colapsar ---
let esperandoSubida = false;

document.addEventListener('click', (e) => {
    const enlace = e.target.closest ? e.target.closest('a[href]') : null;

    if (!enlace || enlace.target === '_blank' || enlace.hasAttribute('download'))
        return;

    const url = new URL(enlace.href, location.href);

    const mismaPagina = url.origin === location.origin &&
        url.pathname === location.pathname &&
        url.search === location.search &&
        !url.hash;

    // Solo importa si el header ya estaba colapsado
    if (mismaPagina && window.scrollY >= alturaHeader()) {
        esperandoSubida = true;
        setTimeout(() => { esperandoSubida = false; }, 1200);
    }
}, true);

// --- Detalle de producto: volver al mismo punto de la lista ---
window.guardarScrollLista = function () {
    window.__scrollLista = window.scrollY;
};

window.restaurarScrollLista = function () {
    const y = window.__scrollLista;

    // Si no hay posicion guardada (enlace directo), solo se colapsa el header
    if (y == null) {
        window.colapsarHeader();
        return;
    }

    const volver = () => window.scrollTo(0, y);

    volver();
    requestAnimationFrame(volver);
    setTimeout(volver, 60);
    setTimeout(volver, 200);

    // Se suelta la posicion cuando ya paso el posible salto de Blazor
    setTimeout(() => {
        if (window.__scrollLista === y) {
            window.__scrollLista = null;
        }
    }, 600);
};

// --- Fondo congelado mientras el detalle esta abierto (Materiales) ---
// Blazor sube la pagina al inicio al navegar a ?id=; aqui se vigila cada frame y se devuelve
// el fondo al punto donde estaba el usuario, asi nunca se ve la pagina arriba detras del modal.
window.bloquearFondo = function () {
    const y = window.scrollY;

    window.__scrollLista = y;
    window.__fondoY = y;
    window.__fondoId = (window.__fondoId || 0) + 1;

    if (window.__fondoLoop)
        return;

    const vigilar = () => {
        const fijo = window.__fondoY;

        // Si ya no hay bloqueo o se salio de Materiales, se deja de vigilar
        if (fijo == null || !/\/materiales$/i.test(location.pathname)) {
            window.__fondoY = null;
            window.__fondoLoop = 0;
            return;
        }

        if (Math.abs(window.scrollY - fijo) > 1) {
            window.scrollTo({ top: fijo, left: 0, behavior: 'instant' });
        }

        window.__fondoLoop = requestAnimationFrame(vigilar);
    };

    window.__fondoLoop = requestAnimationFrame(vigilar);
};

// Se suelta un momento despues de cerrar, para cubrir el salto que hace Blazor al navegar
window.soltarFondo = function () {
    const id = window.__fondoId;

    setTimeout(() => {
        if (window.__fondoId === id) {
            window.__fondoY = null;
        }
    }, 700);
};

window.irASeccion = function (e, id) {
    const destino = document.getElementById(id);

    if (!destino)
        return;

    e.preventDefault();
    e.stopPropagation();

    window.__scrollLista = null;
    destino.scrollIntoView({ behavior: 'smooth', block: 'start' });
};

document.addEventListener('click', e => {
    const a = e.target.closest ? e.target.closest('a[data-seccion]') : null;
    if (a) window.irASeccion(e, a.dataset.seccion);
});

window.addEventListener('scroll', () => {
    // Si Blazor sube al inicio con el detalle abierto (o recien cerrado),
    // se corrige en el mismo instante, antes de que se alcance a ver
    const guardado = window.__scrollLista;

    if (guardado != null &&
        guardado > 1 &&
        window.scrollY <= 1 &&
        /\/(productos|materiales)$/i.test(location.pathname)) {
        window.scrollTo(0, guardado);
    }

    if (esperandoSubida && window.scrollY < alturaHeader()) {
        esperandoSubida = false;
        window.colapsarHeader();
    }

    const header = document.getElementById('mainHeader');

    if (!header)
        return;

    const rect = header.getBoundingClientRect();

    if (rect.bottom <= 70) {
        document.body.classList.add('scrolled');
    } else {
        document.body.classList.remove('scrolled');
    }
});