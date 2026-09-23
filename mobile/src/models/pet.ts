// MODELO: tipos que reflejan exactamente el contrato JSON de la API (snake_case).
// Ver docs/api-contract.md.

export type Pet = {
  id: string;
  pet_type: string;
  name: string | null;
  breed: string | null;
  color_description: string;
  zone: string;
  contact_info: string;
  image_url: string; // URL firmada de lectura (caduca); no guardarla
};

export type CreatePetInput = {
  pet_type: string;
  name?: string;
  breed?: string;
  color_description: string;
  zone: string;
  contact_info: string;
  image_url: string;
};

export const PET_TYPES = ['Perro', 'Gato', 'Otro'] as const;

/** Reglas de validación del cuestionario (espejo de CreatePetRequest en el backend). */
export function validatePetForm(input: Omit<CreatePetInput, 'image_url'>, hasImage: boolean): string | null {
  if (!input.pet_type.trim()) return "El campo 'pet_type' es requerido";
  if (!input.color_description.trim()) return "El campo 'color_description' es requerido";
  if (!input.zone.trim()) return "El campo 'zone' es requerido";
  if (!input.contact_info.trim()) return "El campo 'contact_info' es requerido";
  if (!hasImage) return 'Agrega una foto de la mascota';
  return null;
}
