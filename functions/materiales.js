export async function onRequestGet({ request, env, next }) {
    const url = new URL(request.url);
    const id = parseInt(url.searchParams.get("id") ?? "", 10);
    if (!Number.isInteger(id) || id <= 0) return next();

    try {
        const cfg = await (await env.ASSETS.fetch(new URL("/appsettings.json", url))).json();
        const r = await fetch(
            `${cfg["Supabase:Url"]}/rest/v1/materiales?id=eq.${id}&select=nombre,descripcion,foto&limit=1`,
            { headers: { apikey: cfg["Supabase:Key"] }, cf: { cacheTtl: 3600, cacheEverything: true } }
        );
        if (!r.ok) return next();
        const [p] = await r.json();
        if (!p) return next();

        const res = await next();
        const titulo = `${p.nombre} - Materiales - Muebles Elohim`;
        const desc = (p.descripcion || "").replace(/\s+/g, " ").trim().slice(0, 200);
        const esc = s => String(s ?? "").replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        const canonical = `${cfg.Contacto?.SitioUrl ?? url.origin}/materiales?id=${id}`;

        return new HTMLRewriter()
            .on("title", { element(e) { e.setInnerContent(titulo); } })
            .on('meta[name="description"]', { element(e) { e.setAttribute("content", desc); } })
            .on("head", {
                element(e) {
                    e.append(
                        `<link rel="canonical" href="${esc(canonical)}">` +
                        `<meta property="og:type" content="website">` +
                        `<meta property="og:title" content="${esc(titulo)}">` +
                        `<meta property="og:description" content="${esc(desc)}">` +
                        `<meta property="og:image" content="${esc(p.foto)}">`,
                        { html: true }
                    );
                }
            })
            .transform(res);
    } catch {
        return next();
    }
}