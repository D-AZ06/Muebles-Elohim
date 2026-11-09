using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Muebles_Elohim.Models;
using Supabase.Postgrest;

namespace Muebles_Elohim.Components.Pages
{
    public partial class Productos : ComponentBase
    {
        // Cliente de Supabase registrado en Program.cs
        [Inject]
        public Supabase.Client SupabaseClient { get; set; } = default!;

        // Categorías del filtro (la clave es la que usa CoincideCategoria)
        private record CategoriaFiltro(string Clave, string Nombre, string Icono);

        private readonly List<CategoriaFiltro> categorias = new()
        {
            new("todos", "Todos", "bi-grid-fill"),
            new("entretenimiento", "Centro de entretenimiento", "bi-tv"),
            new("cocina", "Cocina", "bi-cup-hot"),
            new("bano", "Baño", "bi-droplet"),
            new("muebles", "Muebles", "bi-house-door")
        };

        // Lista de productos
        private List<CatalogoFoto> productos = new List<CatalogoFoto>();

        // Categoría seleccionada
        private string categoriaSeleccionada = "todos";

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

        // Consultar información de Supabase
        private async Task CargarProductos()
        {
            try
            {
                cargando = true;
                mensajeError = "";

                var respuesta = await SupabaseClient
                    .From<CatalogoFoto>()
                    .Get();

                Console.WriteLine("Contenido: " + respuesta.Content);
                Console.WriteLine("Cantidad: " + respuesta.Models.Count);

                productos = respuesta.Models
                    .OrderBy(p => p.Id)
                    .ToList();
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
        private void SeleccionarCategoria(string categoria)
        {
            categoriaSeleccionada = categoria;
            panelAbierto = false;
        }

        private void AbrirDetalle(CatalogoFoto producto)
        {
            productoSeleccionado = producto;
            panelAbierto = true;
        }

        private void CerrarDetalle()
        {
            panelAbierto = false;
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

                default:
                    return false;
            }
        }
    }
}