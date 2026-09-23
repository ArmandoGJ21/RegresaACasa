// Variables públicas de Expo (prefijo EXPO_PUBLIC_). Ver .env.example.
export const env = {
  apiUrl: (process.env.EXPO_PUBLIC_API_URL ?? 'http://localhost:5105').replace(/\/$/, ''),
  skipImageUpload: process.env.EXPO_PUBLIC_SKIP_IMAGE_UPLOAD === 'true',
};

export const PLACEHOLDER_IMAGE_URL = 'https://placehold.co/600x400/png?text=Mascota';
