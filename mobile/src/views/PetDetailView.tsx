import { Image, ScrollView, StyleSheet, Text, View } from 'react-native';
import { usePetDetailController } from '../controllers/usePetDetailController';
import { PrimaryButton } from './components/PrimaryButton';
import { colors, spacing } from './theme';

/** VISTA: Detalle de la publicación. */
export function PetDetailView({ petId }: { petId: string }) {
  const { pet, callOwner } = usePetDetailController(petId);

  if (!pet) {
    return (
      <View style={[styles.screen, styles.center]}>
        <Text style={styles.muted}>No existe una publicación con ese id</Text>
      </View>
    );
  }

  return (
    <ScrollView style={styles.screen} contentContainerStyle={styles.content}>
      <Image source={{ uri: pet.image_url }} style={styles.image} />
      <Text style={styles.title}>{pet.name ?? 'Sin nombre'}</Text>
      <Text style={styles.muted}>{[pet.pet_type, pet.breed].filter(Boolean).join(' · ')}</Text>
      <Text style={styles.detail}>🎨 {pet.color_description}</Text>
      <Text style={styles.detail}>📍 Zona: {pet.zone}</Text>

      <View style={styles.action}>
        <PrimaryButton title={`Llamar al ${pet.contact_info}`} onPress={callOwner} />
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  center: { alignItems: 'center', justifyContent: 'center' },
  content: { padding: spacing.lg, gap: spacing.xs },
  image: { width: '100%', aspectRatio: 3 / 2, borderRadius: 12, backgroundColor: colors.border, marginBottom: spacing.md },
  title: { fontSize: 24, fontWeight: '700', color: colors.text },
  muted: { color: colors.muted },
  detail: { color: colors.text, fontSize: 16 },
  action: { marginTop: spacing.xl },
});
