import { useRouter } from 'expo-router';
import { useEffect } from 'react';
import { usePetsStore } from './PetsContext';

/** CONTROLADOR del Muro: carga publicaciones y maneja la navegación. */
export function useFeedController() {
  const router = useRouter();
  const { pets, loading, error, refresh } = usePetsStore();

  useEffect(() => {
    refresh();
  }, [refresh]);

  return {
    pets,
    loading,
    error,
    onRefresh: refresh,
    openPet: (id: string) => router.push({ pathname: '/pets/[id]', params: { id } }),
    openReport: () => router.push('/report'),
  };
}
