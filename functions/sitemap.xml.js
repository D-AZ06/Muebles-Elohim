export async function onRequestGet({ request, env }) {
    const url = new URL(request.url);
    const cfg = await (await env.ASSETS.fetch(new URL("/appsettings.json", url))).json();
    const base = cfg.Contacto?.SitioUrl ?? url.origin;

    let ids = [];
    try {
        const r = await fetch(`${cfg["Supabase:Url"]}/rest/v1/catalogo_fotos?select=id&order=id`, {
            headers: { apikey: cfg["Supabase:Key"] },
            cf: { cacheTtl: 3600, cacheEverything: true }
        });
        if (r.ok) ids = await r.json();
    } catch { }

    const rutas = ["/", "/productos", "/materiales", "/contacto", "/privacidad", ...ids.map(x => `/productos?id=${x.id}`)];
    const xml = `<?xml version="1.0" encoding="UTF-8"?>` +
        `<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">` +
        rutas.map(p => `<url><loc>${base}${p}</loc></url>`).join("") +
        `</urlset>`;

    return new Response(xml, {
        headers: { "Content-Type": "application/xml; charset=utf-8", "Cache-Control": "public, max-age=3600" }
    });
}