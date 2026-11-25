using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Muebles_Elohim.Models;
using Muebles_Elohim.Services;
using Supabase.Postgrest;
using Microsoft.AspNetCore.Components.Web;
using System.Text;

namespace Muebles_Elohim.Components.Pages
{
    public partial class Productos : ComponentBase
    {
        // Cliente de Supabase registrado en Program.cs
        [Inject]
        public Supabase.Client SupabaseClient { get; set; } = default!;

        [Inject]
        private NavigationManager Navegacion { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        private IConfiguration Configuracion { get; set; } = default!;

        private bool menuCompartirAbierto;

        [Inject]
        private ILogger<Productos> Logger { get; set; } = default!;

        [Inject]
        private CatalogoServicio Catalogo { get; set; } = default!;

        // Id que viene en la URL: /productos?id=12
        [SupplyParameterFromQuery(Name = "id")]
        public int? IdProducto { get; set; }

        private bool enlaceCopiado;

        private ElementReference modalRef;
        private bool enfocarModal;

        private bool productoNoDisponible;

        private bool animar;

        private string TituloPagina
        {
            get
            {
                if (panelAbierto && productoSeleccionado is not null)
                {
                    return productoSeleccionado.Titulo + " - Muebles Elohim";
                }

                return "Productos - Muebles Elohim";
            }
        }

        // Categorías del filtro (la clave es la que usa CoincideCategoria)
        private record CategoriaFiltro(string Clave, string Nombre, string Icono);

        private readonly List<CategoriaFiltro> categorias = new()
        {
            new("todos", "Todos", "bi-grid-fill"),
            new("entretenimiento", "Centro de entretenimiento", "bi-tv"),
            new("cocina", "Cocina", "bi-cup-hot"),
            new("bano", "Baño", "bi-droplet"),
            new("muebles", "Muebles", "bi-house-door"),
            new("closet", "Closet", "bi-layout-text-sidebar-reverse")
        };

        // Lista de productos
        private List<CatalogoFoto> productos = new List<CatalogoFoto>();

        // Categoría seleccionada
        private string categoriaSeleccionada = "todos";

        private const int ProductosPorPagina = 12;
        private int paginaActual = 1;

        private int TotalPaginas =>
            Math.Max(1, (int)Math.Ceiling(ProductosFiltrados.Count / (double)ProductosPorPagina));

        private List<CatalogoFoto> ProductosPagina =>
            ProductosFiltrados
                .Skip((paginaActual - 1) * ProductosPorPagina)
                .Take(ProductosPorPagina)
                .ToList();

        private async Task IrAPagina(int pagina)
        {
            animar = true;
            paginaActual = Math.Clamp(pagina, 1, TotalPaginas);
            await JS.InvokeVoidAsync("colapsarHeader");
        }

        // Panel de detalle (el producto se conserva al cerrar para que no se vacíe durante la animación)
        private CatalogoFoto? productoSeleccionado;
        private bool panelAbierto;

        // Estados de la página
        private bool cargando = true;
        private string mensajeError = "";

        // Lista de productos filtrados
        private List<CatalogoFoto> ProductosFiltrados
        {
            get
            {
                if (categoriaSeleccionada == "todos")
                {
                    return productos;
                }

                return productos.Where(p =>
                    CoincideCategoria(p.Categoria, categoriaSeleccionada)
                ).ToList();
            }
        }

        // Cargar productos al iniciar la página
        protected override async Task OnInitializedAsync()
        {
            animar = Animacion.Activo;
            await CargarProductos();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (enfocarModal && panelAbierto)
            {
                enfocarModal = false;
                await modalRef.FocusAsync();
            }
        }

        private void CambiarProducto(int paso)
        {
            menuCompartirAbierto = false;

            var lista = ProductosFiltrados;
            int i = lista.FindIndex(p => p.Id == productoSeleccionado?.Id);
            if (i < 0) return;

            int nuevo = (i + paso + lista.Count) % lista.Count;
            Navegacion.NavigateTo("productos?id=" + lista[nuevo].Id);
        }

        private void TeclaModal(KeyboardEventArgs e)
        {
            if (!panelAbierto) return;

            if (e.Key == "Escape") CerrarDetalle();
            else if (e.Key == "ArrowRight") CambiarProducto(1);
            else if (e.Key == "ArrowLeft") CambiarProducto(-1);
        }

        // Consultar información de Supabase
        private async Task CargarProductos()
        {
            try
            {
                cargando = true;
                mensajeError = "";
                
                /*
                var respuesta = await SupabaseClient
                    .From<CatalogoFoto>()
                    .Get();

                productos = respuesta.Models
                    .OrderBy(p => p.Id)
                    .ToList();
                */

                productos = await Catalogo.ObtenerAsync();

                SincronizarDetalleConUrl();
            }
            catch (Exception ex)
            {
                // El detalle técnico queda solo en el log del servidor
                Logger.LogError(ex, "Error al cargar el catálogo de productos");

                mensajeError = "No pudimos cargar los productos en este momento. " +
                               "Por favor intenta de nuevo en unos minutos.";
            }
            finally
            {
                cargando = false;
            }
        }
        private async Task SeleccionarCategoria(string categoria)
        {
            animar = true;
            categoriaSeleccionada = categoria;
            paginaActual = 1;
            panelAbierto = false;

            if (IdProducto is not null)
            {
                Navegacion.NavigateTo("productos");
            }

            await JS.InvokeVoidAsync("colapsarHeader");
        }

        private void AbrirDetalle(CatalogoFoto producto)
        {
            productoSeleccionado = producto;
            panelAbierto = true;
            enlaceCopiado = false;
            menuCompartirAbierto = false;

            Navegacion.NavigateTo("productos?id=" + producto.Id);
        }

        private void CerrarDetalle()
        {
            panelAbierto = false;
            menuCompartirAbierto = false;

            if (IdProducto is not null)
            {
                Navegacion.NavigateTo("productos");
            }
        }

        // Cantidad de productos por categoría (para el contador del filtro)
        private int ContarPorCategoria(string clave)
        {
            if (clave == "todos")
            {
                return productos.Count;
            }

            return productos.Count(p => CoincideCategoria(p.Categoria, clave));
        }

        // Comprobar si el producto pertenece a la categoría
        private bool CoincideCategoria(string? categoria, string clave)
        {
            string nombre = (categoria ?? "").Trim().ToLowerInvariant();

            switch (clave)
            {
                case "entretenimiento":
                    return nombre.Contains("entretenimiento");

                case "cocina":
                    return nombre.Contains("cocina");

                case "bano":
                    return nombre.Contains("baño") ||
                           nombre.Contains("bano");

                case "muebles":
                    return nombre.Contains("muebles");

                case "closet":
                    return nombre.Contains("closet");

                default:
                    return false;
            }
        }

        // La URL manda: si trae ?id=, se abre ese producto; si no, se cierra el detalle
        protected override void OnParametersSet()
        {
            SincronizarDetalleConUrl();
        }

        private void SincronizarDetalleConUrl()
        {
            menuCompartirAbierto = false;
            productoNoDisponible = false;

            if (IdProducto is null)
            {
                panelAbierto = false;
                return;
            }

            CatalogoFoto? encontrado = productos.FirstOrDefault(p => p.Id == IdProducto.Value);

            if (encontrado is null)
            {
                panelAbierto = false;

                // Solo se avisa si el catálogo cargó bien (si falló, ya sale otro mensaje)
                productoNoDisponible = !cargando && string.IsNullOrEmpty(mensajeError) && productos.Count > 0;
                return;
            }

            productoSeleccionado = encontrado;
            panelAbierto = true;
            enlaceCopiado = false;
            enfocarModal = true;
        }

        private void AlternarMenuCompartir()
        {
            menuCompartirAbierto = !menuCompartirAbierto;
        }

        private void AbrirMenuCompartir()
        {
            menuCompartirAbierto = true;
        }

        private void CerrarMenuCompartir()
        {
            menuCompartirAbierto = false;
        }

        private string ObtenerEnlaceProducto()
        {
            if (productoSeleccionado is null)
            {
                return Navegacion.ToAbsoluteUri("productos").ToString();
            }

            return Navegacion.ToAbsoluteUri($"productos?id={productoSeleccionado.Id}").ToString();
        }

        private async Task CotizarPorWhatsApp()
        {
            if (productoSeleccionado is null)
            {
                return;
            }

            menuCompartirAbierto = false;

            // Lee "573175223193" desde secrets.json -> Contacto:WhatsApp
            string numeroWhatsApp = Configuracion["Contacto:WhatsApp"] ?? "573175223193";
            string numeroLimpio = new string(numeroWhatsApp.Where(char.IsDigit).ToArray());

            string enlace = ObtenerEnlaceProducto();
            var sb = new StringBuilder();
            sb.AppendLine("Hola, me gustaría cotizar este mueble de *Muebles Elohim*:");
            sb.AppendLine($"• *Producto:* {productoSeleccionado.Titulo}");

            if (!string.IsNullOrWhiteSpace(productoSeleccionado.Categoria))
            {
                sb.AppendLine($"• *Categoría:* {productoSeleccionado.Categoria}");
            }

            sb.AppendLine($"• *Enlace:* {enlace}");

            string mensajeCodificado = Uri.EscapeDataString(sb.ToString());
            string urlWhatsApp = $"https://wa.me/{numeroLimpio}?text={mensajeCodificado}";

            try
            {
                await JS.InvokeVoidAsync("window.open", urlWhatsApp, "_blank", "noopener,noreferrer");
            }
            catch (JSException)
            {
            }
            catch (JSDisconnectedException)
            {
            }
        }

        private async Task CompartirEnOtrasApps()
        {
            if (productoSeleccionado is null)
            {
                return;
            }

            menuCompartirAbierto = false;

            string enlace = ObtenerEnlaceProducto();
            string titulo = $"{productoSeleccionado.Titulo} - Muebles Elohim";
            string texto = $"Mira este mueble de Muebles Elohim: {productoSeleccionado.Titulo}";

            try
            {
                bool compartido = await JS.InvokeAsync<bool>("compartirProductoNativo", titulo, texto, enlace);

                if (!compartido)
                {
                    await CopiarEnlace();
                }
            }
            catch (JSException)
            {
                await CopiarEnlace();
            }
            catch (JSDisconnectedException)
            {
            }
        }

        private async Task CopiarEnlace()
        {
            if (productoSeleccionado is null)
            {
                return;
            }

            string enlace = ObtenerEnlaceProducto();

            try
            {
                await JS.InvokeVoidAsync("navigator.clipboard.writeText", enlace);

                enlaceCopiado = true;
                menuCompartirAbierto = false;

                StateHasChanged();

                await Task.Delay(2000);

                enlaceCopiado = false;

                StateHasChanged(); // <- para ocultar el mensaje
            }
            catch (JSException)
            {
            }
        }

        private string IconoDe(string categoria)
        {
            foreach (CategoriaFiltro c in categorias)
            {
                if (c.Clave != "todos" && CoincideCategoria(categoria, c.Clave))
                {
                    return c.Icono;
                }
            }

            return "bi-tag";
        }
    }
}