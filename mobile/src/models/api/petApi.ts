import type { CreateCommentInput, CreatePetInput, Pet, PetComment } from '../pet';
import { request } from './httpClient';

export const petApi = {
  /** GET /api/v1/pets */
  list: () => request<Pet[]>('/api/v1/pets'),

  /** POST /api/v1/pets */
  create: (input: CreatePetInput) =>
    request<Pet>('/api/v1/pets', { method: 'POST', body: JSON.stringify(input) }),

  /** POST /api/v1/pets/:id/comments */
  addComment: (petId: string, input: CreateCommentInput) =>
    request<PetComment>(`/api/v1/pets/${encodeURIComponent(petId)}/comments`, {
      method: 'POST',
      body: JSON.stringify(input),
    }),
};
