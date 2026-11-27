using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Muebles_Elohim.Models;
using Muebles_Elohim.Services;

namespace Muebles_Elohim.Components.Pages
{
    public partial class Materiales : ComponentBase
    {
        [Inject]
        private MaterialesServicio MaterialesSrv { get; set; } = default!;

        [Inject]
        private NavigationManager Navegacion { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        private IConfiguration Configuracion { get; set; } = default!;

        [SupplyParameterFromQuery(Name = "id")]
        public int? IdMaterial { get; set; }

        private record Grupo(string Ancla, string Titulo, string Icono, List<Material> Items);

        private static readonly string[] OrdenTipos = { "textura", "madera", "color" };

        private List<Material> materiales = new();
        private List<Grupo> grupos = new();
        private List<Material> ordenados = new();

        private Material? seleccionado;
        private bool cargando = true;
        private bool panelAbierto;
        private bool menuCompartirAbierto;
        private bool enlaceCopiado;
        private bool enfocar;
        private bool panelEstabaAbierto;

        private ElementReference modalRef;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                materiales = await MaterialesSrv.ObtenerAsync();
            }
            catch (Exception)
            {
                materiales = new();
            }

            Armar();
            cargando = false;
            Sincronizar();
        }

        protected override void OnParametersSet()
        {
            Sincronizar();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (enfocar && panelAbierto)
            {
                enfocar = false;
                await modalRef.FocusAsync();
            }

            // Al cerrar el detalle se vuelve al mismo punto de la lista
            if (panelEstabaAbierto && !panelAbierto)
            {
                panelEstabaAbierto = false;
                await JS.InvokeVoidAsync("restaurarScrollLista");
                await JS.InvokeVoidAsync("soltarFondo");
            }
            else if (panelAbierto)
            {
                panelEstabaAbierto = true;
            }
        }

        // Texturas primero, luego maderas, colores y cualquier otro tipo
        private void Armar()
        {
            grupos = materiales
                .GroupBy(m => ClaveTipo(m.Tipo))
                .OrderBy(g => { int i = Array.IndexOf(OrdenTipos, g.Key); return i < 0 ? 99 : i; })
                .ThenBy(g => g.Key)
                .Select(g => new Grupo("sec-" + g.Key.Replace(' ', '-'), TituloTipo(g.Key), IconoTipo(g.Key), g.OrderBy(m => m.Id).ToList()))
                .ToList();

            ordenados = grupos.SelectMany(g => g.Items).ToList();
        }

        private static string ClaveTipo(string? tipo)
        {
            string t = (tipo ?? "").Trim().ToLowerInvariant();

            return t switch
            {
                "" or "otros" => "otros",
                "textura" or "texturas" => "textura",
                "madera" or "maderas" => "madera",
                "color" or "colores" => "color",
                _ => t
            };
        }


        private static string TituloTipo(string? tipo)
        {
            string t = (tipo ?? "").Trim().ToLowerInvariant();

            return t switch
            {
                "" or "otros" => "Otros",
                "textura" => "Texturas",
                "madera" => "Maderas",
                "color" => "Colores",
                _ => char.ToUpperInvariant(t[0]) + t.Substring(1)
            };
        }

        private static string IconoTipo(string tipo) => tipo switch
        {
            "textura" => "bi-grid-3x3-gap",
            "madera" => "bi-tree",
            "color" => "bi-palette",
            _ => "bi-tag"
        };

        // Prefijo "mat-" para que no choque con los ids de productos en favoritos y cotización
        private static string IdDe(Material m) => "mat-" + m.Id;

        private void Sincronizar()
        {
            menuCompartirAbierto = false;

            Material? encontrado = IdMaterial is null ? null : materiales.FirstOrDefault(x => x.Id == IdMaterial.Value);

            if (encontrado is null)
            {
                panelAbierto = false;
                return;
            }

            seleccionado = encontrado;
            panelAbierto = true;
            enlaceCopiado = false;
            enfocar = true;
        }

        private async Task AbrirDetalle(Material m)
        {
            // Se guarda en qué punto de la lista estaba el usuario
            await JS.InvokeVoidAsync("bloquearFondo");

            Navegacion.NavigateTo("materiales?id=" + m.Id);
        }

        private void CerrarDetalle()
        {
            panelAbierto = false;
            menuCompartirAbierto = false;

            if (IdMaterial is not null)
            {
                Navegacion.NavigateTo("materiales");
            }
        }

        // Solo el mouse abre/cierra por hover; en pantallas táctiles el menú lo maneja el clic
        private void AbrirMenuCompartir(PointerEventArgs e)
        {
            if (e.PointerType == "mouse") menuCompartirAbierto = true;
        }

        private void CerrarMenuCompartir(PointerEventArgs e)
        {
            if (e.PointerType == "mouse") menuCompartirAbierto = false;
        }

        private void CambiarMaterial(int paso)
        {
            int i = ordenados.FindIndex(m => m.Id == seleccionado?.Id);
            if (i < 0) return;

            int nuevo = (i + paso + ordenados.Count) % ordenados.Count;
            Navegacion.NavigateTo("materiales?id=" + ordenados[nuevo].Id);
        }

        private void TeclaModal(KeyboardEventArgs e)
        {
            if (!panelAbierto) return;

            if (e.Key == "Escape") CerrarDetalle();
            else if (e.Key == "ArrowRight") CambiarMaterial(1);
            else if (e.Key == "ArrowLeft") CambiarMaterial(-1);
        }

        private string Enlace() =>
            Navegacion.ToAbsoluteUri("materiales" + (seleccionado is null ? "" : "?id=" + seleccionado.Id)).ToString();

        private async Task PreguntarPorWhatsApp()
        {
            if (seleccionado is null) return;
            menuCompartirAbierto = false;

            string numero = Configuracion["Contacto:WhatsApp"] ?? "573175223193";
            numero = new string(numero.Where(char.IsDigit).ToArray());

            var sb = new StringBuilder();
            sb.AppendLine("Hola, me interesa este material de *Muebles Elohim*:");
            sb.AppendLine($"• *Material:* {seleccionado.Nombre}");
            sb.AppendLine($"• *Tipo:* {TituloTipo(seleccionado.Tipo)}");
            sb.AppendLine($"• *Enlace:* {Enlace()}");

            try
            {
                await JS.InvokeVoidAsync("window.open", $"https://wa.me/{numero}?text={Uri.EscapeDataString(sb.ToString())}", "_blank", "noopener,noreferrer");
            }
            catch (JSException) { }
            catch (JSDisconnectedException) { }
        }

        private async Task CompartirEnOtrasApps()
        {
            if (seleccionado is null) return;
            menuCompartirAbierto = false;

            try
            {
                bool ok = await JS.InvokeAsync<bool>("compartirProductoNativo",
                    $"{seleccionado.Nombre} - Muebles Elohim",
                    $"Mira este material de Muebles Elohim: {seleccionado.Nombre}",
                    Enlace());

                if (!ok) await CopiarEnlace();
            }
            catch (JSException)
            {
                await CopiarEnlace();
            }
            catch (JSDisconnectedException) { }
        }

        private async Task CopiarEnlace()
        {
            if (seleccionado is null) return;

            try
            {
                await JS.InvokeVoidAsync("navigator.clipboard.writeText", Enlace());
                enlaceCopiado = true;
                menuCompartirAbierto = false;
                StateHasChanged();

                await Task.Delay(2000);

                enlaceCopiado = false;
                StateHasChanged();
            }
            catch (JSException) { }
        }
    }
}