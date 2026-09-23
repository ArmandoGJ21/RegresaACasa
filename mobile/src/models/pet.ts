// MODELO: tipos que reflejan exactamente el contrato JSON de la API (snake_case).
// Ver docs/api-contract.md.

export type PetComment = {
  user_name: string;
  text: string;
  created_at: string; // ISO 8601 (UTC)
};

export type Pet = {
  id: string;
  pet_type: string;
  name: string | null;
  breed: string | null;
  color_description: string;
  zone: string;
  contact_info: string;
  image_url: string;
  comments: PetComment[];
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

export type CreateCommentInput = {
  user_name: string;
  text: string;
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

export function validateComment(input: CreateCommentInput): string | null {
  if (!input.user_name.trim()) return "El campo 'user_name' es requerido";
  if (!input.text.trim()) return "El campo 'text' es requerido";
  return null;
}
