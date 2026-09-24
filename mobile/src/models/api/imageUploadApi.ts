import { Platform } from 'react-native';
import { env, PLACEHOLDER_IMAGE_URL } from '../../config/env';
import { ApiError, request } from './httpClient';

type ImageUploadTicket = {
  upload_url: string;
  image_url: string;
  expires_at: string;
};

/**
 * Pasos 1 y 2 del flujo: sube la foto directo a Azure Blob Storage y devuelve su image_url.
 * La API solo firma la URL (SAS); el archivo nunca pasa por el backend.
 */
export async function uploadPetImage(localUri: string, contentType: string): Promise<string> {
  if (env.skipImageUpload) {
    return PLACEHOLDER_IMAGE_URL;
  }

  const ticket = await request<ImageUploadTicket>('/api/v1/uploads/images', {
    method: 'POST',
    body: JSON.stringify({ content_type: contentType }),
  });

  const file = await (await fetch(localUri)).blob();
  let response: Response;
  try {
    response = await fetch(ticket.upload_url, {
      method: 'PUT',
      headers: { 'x-ms-blob-type': 'BlockBlob', 'Content-Type': contentType },
      body: file,
    });
  } catch {
    // En la versión web, un fallo aquí casi siempre es el CORS de Azure (infra/configure-cors.cs).
    if (__DEV__ && Platform.OS === 'web') {
      console.warn('Azure rechazó la subida desde el navegador: configura CORS con `dotnet run infra/configure-cors.cs`.');
    }
    throw new ApiError('No se pudo subir la foto. Revisa tu conexión e intenta de nuevo.', 0);
  }

  if (!response.ok) {
    throw new ApiError('No se pudo subir la foto', response.status);
  }
  return ticket.image_url;
}
