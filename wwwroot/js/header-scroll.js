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