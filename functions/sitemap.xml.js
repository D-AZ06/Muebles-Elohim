export async function onRequestGet({ request, env }) {
    const url = new URL(request.url);
    const cfg = await (await env.ASSETS.fetch(new URL("/appsettings.json", url))).json();
    const base = cfg.Contacto?.SitioUrl ?? url.origin;

    const ids = async (tabla) => {
        try {
            const r = await fetch(`${cfg["Supabase:Url"]}/rest/v1/${tabla}?select=id&order=id`, {
                headers: { apikey: cfg["Supabase:Key"] },
                cf: { cacheTtl: 3600, cacheEverything: true }
            });
            return r.ok ? await r.json() : [];
        } catch { return []; }
    };
    const [productos, materiales] = await Promise.all([ids("catalogo_fotos"), ids("materiales")]);

    const rutas = [
        "/", "/productos", "/materiales", "/contacto", "/privacidad",
        ...productos.map(x => `/productos?id=${x.id}`),
        ...materiales.map(x => `/materiales?id=${x.id}`)
    ];
    const xml = `<?xml version="1.0" encoding="UTF-8"?>` +
        `<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">` +
        rutas.map(p => `<url><loc>${base}${p}</loc></url>`).join("") +
        `</urlset>`;

    return new Response(xml, {
        headers: { "Content-Type": "application/xml; charset=utf-8", "Cache-Control": "public, max-age=3600" }
    });
}