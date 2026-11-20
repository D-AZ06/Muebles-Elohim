window.compartirProductoNativo = async function (title, text, url) {
    if (navigator.share) {
        try {
            await navigator.share({ title, text, url });
            return true;
        } catch (err) {
            if (err && err.name === "AbortError") {
                return true;
            }
            return false;
        }
    }
    return false;
};