using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Muebles_Elohim.Models;
using Muebles_Elohim.Services;
using Supabase.Postgrest;
using Microsoft.AspNetCore.Components.Web;

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
        private CatalogoServicio Catalogo { get; set; } = default!;

        // Id que viene en la URL: /productos?id=12
        [SupplyParameterFromQuery(Name = "id")]
        public int? IdProducto { get; set; }

        private bool enlaceCopiado;

        private ElementReference modalRef;
        private bool enfocarModal;

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
            paginaActual = Math.Clamp(pagina, 1, TotalPaginas);
            await JS.InvokeVoidAsync("window.scrollTo", 0, 0);
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
                mensajeError = "No se pudieron cargar los productos: " + ex.Message;
            }
            finally
            {
                cargando = false;
            }
        }
        private async Task SeleccionarCategoria(string categoria)
        {
            categoriaSeleccionada = categoria;
            paginaActual = 1;
            panelAbierto = false;

            if (IdProducto is not null)
            {
                Navegacion.NavigateTo("productos");
            }

            await JS.InvokeVoidAsync("window.scrollTo", 0, 0);
        }

        private void AbrirDetalle(CatalogoFoto producto)
        {
            productoSeleccionado = producto;
            panelAbierto = true;
            enlaceCopiado = false;

            Navegacion.NavigateTo("productos?id=" + producto.Id);
        }

        private void CerrarDetalle()
        {
            panelAbierto = false;

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
            if (IdProducto is null)
            {
                panelAbierto = false;
                return;
            }

            CatalogoFoto? encontrado = productos.FirstOrDefault(p => p.Id == IdProducto.Value);

            if (encontrado is null)
            {
                panelAbierto = false;
                return;
            }

            productoSeleccionado = encontrado;
            panelAbierto = true;
            enlaceCopiado = false;
            enfocarModal = true;
        }

        private async Task CopiarEnlace()
        {
            if (productoSeleccionado is null)
            {
                return;
            }

            string enlace = Navegacion.ToAbsoluteUri("productos?id=" + productoSeleccionado.Id).ToString();

            try
            {
                await JS.InvokeVoidAsync("navigator.clipboard.writeText", enlace);
                enlaceCopiado = true;
                StateHasChanged();

                await Task.Delay(2000);
                enlaceCopiado = false;
            }
            catch (JSException)
            {
                // El navegador no permitió copiar
            }
            catch (JSDisconnectedException)
            {
                // El circuito se cerró
            }
        }
    }
}