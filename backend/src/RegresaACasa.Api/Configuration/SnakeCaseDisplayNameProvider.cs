using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace RegresaACasa.Api.Configuration;

/// <summary>
/// Hace que "{0}" en los mensajes de validación sea el nombre JSON del campo
/// ("El campo 'zone' es requerido" en lugar de "'Zone'").
/// </summary>
public class SnakeCaseDisplayNameProvider : IDisplayMetadataProvider
{
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        if (context.Key.MetadataKind == ModelMetadataKind.Property && context.DisplayMetadata.DisplayName is null)
        {
            var jsonName = JsonNamingPolicy.SnakeCaseLower.ConvertName(context.Key.Name!);
            context.DisplayMetadata.DisplayName = () => jsonName;
        }
    }
}
