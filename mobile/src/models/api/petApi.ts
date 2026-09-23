import type { CreatePetInput, Pet } from '../pet';
import { request } from './httpClient';

export const petApi = {
  /** GET /api/v1/pets */
  list: () => request<Pet[]>('/api/v1/pets'),

  /** POST /api/v1/pets */
  create: (input: CreatePetInput) =>
    request<Pet>('/api/v1/pets', { method: 'POST', body: JSON.stringify(input) }),
};
