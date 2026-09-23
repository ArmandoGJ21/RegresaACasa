import { useLocalSearchParams } from 'expo-router';
import { PetDetailView } from '../../views/PetDetailView';

export default function PetDetailRoute() {
  const { id } = useLocalSearchParams<{ id: string }>();
  return <PetDetailView petId={id} />;
}
