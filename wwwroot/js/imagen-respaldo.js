(function () {
    const RESPALDO = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(
        '<svg xmlns="http://www.w3.org/2000/svg" width="400" height="300" viewBox="0 0 400 300">' +
        '<rect width="400" height="300" fill="#e9ecef"/>' +
        '<text x="200" y="155" text-anchor="middle" font-family="sans-serif" font-size="20" fill="#6c757d">' +
        'Imagen no disponible</text></svg>');

    function aplicarRespaldo(img) {
        // Si ya tiene el respaldo (data:) no se repite, así nunca hay bucles
        if (!img.src || img.src.startsWith('data:')) return;
        img.src = RESPALDO;
    }

    // Los eventos "error" de las imágenes no suben por el DOM: se escuchan en la fase de captura
    document.addEventListener('error', e => {
        if (e.target && e.target.tagName === 'IMG') aplicarRespaldo(e.target);
    }, true);

    // Imágenes que fallaron antes de que este script cargara
    window.addEventListener('load', () => {
        document.querySelectorAll('img').forEach(img => {
            if (img.complete && img.naturalWidth === 0) aplicarRespaldo(img);
        });
    });
})();