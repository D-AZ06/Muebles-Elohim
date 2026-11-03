using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Muebles_Elohim.Components.Pages;

public partial class Home : ComponentBase, IDisposable
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private int currentIndex = 0;
    private int slideInterval = 4000;

    private CancellationTokenSource? cts;

    private class SlideInfo
    {
        public string ImageUrl { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
    }

    private SlideInfo[] slides =
    {
        new()
        {
            ImageUrl="https://dhqcnrgknozpixhplobn.supabase.co/storage/v1/object/public/Carrusel/televisor.jpeg",
            Title="Centros de TV y Paneles Flotantes",
            Description="Diseños empotrados a la pared con acabados modernos para una sala limpia y sofisticada."
        },

        new()
        {
            ImageUrl="https://dhqcnrgknozpixhplobn.supabase.co/storage/v1/object/public/Carrusel/cocina.jpeg",
            Title="Cocinas Integrales a Medida",
            Description="Gabinetes y alacenas funcionales que aprovechan al máximo cada espacio de tu cocina."
        },

        new()
        {
            ImageUrl="https://dhqcnrgknozpixhplobn.supabase.co/storage/v1/object/public/Carrusel/closet.jpeg",
            Title="Muebles a Medida y Closets",
            Description="Clósets y muebles a medida que combinan la máxima organización con un estilo elegante."
        }
    };

    protected override async Task OnInitializedAsync()
    {
        cts = new CancellationTokenSource();

        _ = IniciarCicloAutomatico(cts.Token);

        await Task.CompletedTask;
    }

    private async Task IniciarCicloAutomatico(
        CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(slideInterval, token);

                currentIndex =
                    (currentIndex + 1) % slides.Length;

                await InvokeAsync(StateHasChanged);
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void SeleccionarSlide(int index)
    {
        currentIndex = index;

        cts?.Cancel();

        cts = new CancellationTokenSource();

        _ = IniciarCicloAutomatico(cts.Token);

        StateHasChanged();
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}