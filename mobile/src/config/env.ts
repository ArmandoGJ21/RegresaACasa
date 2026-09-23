// Variables públicas de Expo (prefijo EXPO_PUBLIC_). Ver mobile/.env.example.
//
// RESTRICCIÓN: todo lo que empieza con EXPO_PUBLIC_ queda incrustado en la app y
// cualquiera puede leerlo. Nunca pongas aquí llaves de Azure ni contraseñas; la app
// obtiene permisos de subida pidiendo una URL SAS temporal a la API.
//
// Si una variable es inválida la app falla al abrir con un mensaje claro.

const HTTP_URL = /^https?:\/\/[^\s/?#]+(:\d+)?(\/[^\s?#]*)?$/i;

class EnvError extends Error {
  constructor(message: string) {
    super(`[mobile/.env.local] ${message}`);
  }
}

function readApiUrl(): string {
  const value = process.env.EXPO_PUBLIC_API_URL?.trim();
  if (!value) {
    throw new EnvError('Falta EXPO_PUBLIC_API_URL. Copia mobile/.env.example a mobile/.env.local.');
  }
  if (!HTTP_URL.test(value)) {
    throw new EnvError(`EXPO_PUBLIC_API_URL debe ser una URL http(s) sin query, p. ej. http://192.168.1.10:5105 (valor: "${value}").`);
  }
  if (!__DEV__ && !value.toLowerCase().startsWith('https://')) {
    throw new EnvError('EXPO_PUBLIC_API_URL debe usar https en builds de producción.');
  }
  return value.replace(/\/+$/, '');
}

function readSkipImageUpload(): boolean {
  const value = process.env.EXPO_PUBLIC_SKIP_IMAGE_UPLOAD?.trim().toLowerCase();
  if (value === undefined || value === '' || value === 'false') {
    return false;
  }
  if (value !== 'true') {
    throw new EnvError(`EXPO_PUBLIC_SKIP_IMAGE_UPLOAD solo acepta "true" o "false" (valor: "${value}").`);
  }
  if (!__DEV__) {
    throw new EnvError('EXPO_PUBLIC_SKIP_IMAGE_UPLOAD=true solo se permite en desarrollo.');
  }
  return true;
}

export const env = {
  apiUrl: readApiUrl(),
  skipImageUpload: readSkipImageUpload(),
};

export const PLACEHOLDER_IMAGE_URL = 'https://placehold.co/600x400/png?text=Mascota';
