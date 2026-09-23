import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';
import { petApi } from '../models/api/petApi';
import type { Pet } from '../models/pet';

// Estado compartido del muro entre pantallas. Los controladores lo consumen;
// las vistas nunca hablan con la API directamente.

type PetsState = {
  pets: Pet[];
  loading: boolean;
  error: string | null;
  refresh: () => Promise<void>;
  addPet: (pet: Pet) => void;
};

const PetsContext = createContext<PetsState | null>(null);

export function PetsProvider({ children }: { children: ReactNode }) {
  const [pets, setPets] = useState<Pet[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setPets(await petApi.list());
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al cargar publicaciones');
    } finally {
      setLoading(false);
    }
  }, []);

  const addPet = useCallback((pet: Pet) => setPets((current) => [pet, ...current]), []);

  const value = useMemo(
    () => ({ pets, loading, error, refresh, addPet }),
    [pets, loading, error, refresh, addPet],
  );

  return <PetsContext.Provider value={value}>{children}</PetsContext.Provider>;
}

export function usePetsStore(): PetsState {
  const ctx = useContext(PetsContext);
  if (!ctx) throw new Error('usePetsStore debe usarse dentro de <PetsProvider>');
  return ctx;
}
