// Altura del header grande (el que se esconde al hacer scroll)
function alturaHeader() {
    const header = document.getElementById('mainHeader');
    return header ? header.offsetHeight : 0;
}

// Deja la pagina con el header grande escondido y la barra sticky arriba.
// Si el usuario hace scroll hacia arriba, el header se muestra normal.
window.colapsarHeader = function () {
    window.__scrollLista = null;
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

window.addEventListener('scroll', () => {
    // Si Blazor sube al inicio con el detalle abierto (o recien cerrado),
    // se corrige en el mismo instante, antes de que se alcance a ver
    const guardado = window.__scrollLista;

    if (guardado != null &&
        guardado > 1 &&
        window.scrollY <= 1 &&
        location.pathname.toLowerCase().endsWith('/productos')) {
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