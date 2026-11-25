// Altura del header grande (el que se esconde al hacer scroll)
function alturaHeader() {
    const header = document.getElementById('mainHeader');
    return header ? header.offsetHeight : 0;
}

// Deja la pagina con el header grande escondido y la barra sticky arriba.
// Si el usuario hace scroll hacia arriba, el header se muestra normal.
window.colapsarHeader = function () {
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

window.addEventListener('scroll', () => {
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