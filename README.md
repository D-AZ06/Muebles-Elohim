# Muebles Elohim
Catálogo web (Blazor Server, .NET 8) con lista de cotización que se envía por WhatsApp.

## Configuración local
```
dotnet user-secrets set "Supabase:Url" "https://TU-PROYECTO.supabase.co"
dotnet user-secrets set "Supabase:Key" "CLAVE_ANON"
dotnet user-secrets set "Contacto:WhatsApp" "57300XXXXXXX"
dotnet user-secrets set "Contacto:Telefono" "..."
dotnet user-secrets set "Contacto:Correo" "..."
dotnet user-secrets set "Contacto:SitioUrl" "https://tu-dominio"
dotnet run
```
En producción se usan variables de entorno: `Supabase__Url`, `Supabase__Key`, `Contacto__WhatsApp`, etc.

## Base de datos
Tabla `catalogo_fotos` (id, foto, titulo, categoria, descripcion), con RLS de solo lectura.