import { Linking } from 'react-native';
import { usePetsStore } from './PetsContext';

/** CONTROLADOR del detalle: muestra una publicación y permite llamar al dueño. */
export function usePetDetailController(petId: string) {
  const { pets } = usePetsStore();
  const pet = pets.find((p) => p.id === petId) ?? null;

  const callOwner = () => {
    if (pet) {
      Linking.openURL(`tel:${pet.contact_info}`);
    }
  };

  return { pet, callOwner };
}
