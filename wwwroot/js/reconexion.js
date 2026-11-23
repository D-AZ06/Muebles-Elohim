(function () {
    document.addEventListener('click', e => {
        if (e.target.closest('#btnRecargar')) location.reload();
    });

    const modal = document.getElementById('components-reconnect-modal');
    if (!modal) return;

    new MutationObserver(() => {
        // El servidor se reinició y se perdió el estado: recargar es lo único que sirve
        if (modal.classList.contains('components-reconnect-rejected')) location.reload();
    }).observe(modal, { attributes: true, attributeFilter: ['class'] });
})();