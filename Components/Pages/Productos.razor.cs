
using Microsoft.AspNetCore.Components;
using Muebles_Elohim.Models;
using Supabase.Postgrest;

namespace Muebles_Elohim.Components.Pages
{
    public partial class Productos : ComponentBase
    {
        // Cliente de Supabase registrado en Program.cs
        [Inject]
        public Supabase.Client SupabaseClient { get; set; } = default!;

        // Lista de productos
        private List<CatalogoFoto> productos = new List<CatalogoFoto>();

        // Categoría seleccionada
        private string categoriaSeleccionada = "todos";

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
                    CoincideCategoria(p.Categoria)
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

        // Seleccionar categoría
        private void SeleccionarCategoria(string categoria)
        {
            categoriaSeleccionada = categoria;
        }

        // Comprobar si el producto pertenece a la categoría
        private bool CoincideCategoria(string categoria)
        {
            string nombre = (categoria ?? "").Trim().ToLowerInvariant();

            switch (categoriaSeleccionada)
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
