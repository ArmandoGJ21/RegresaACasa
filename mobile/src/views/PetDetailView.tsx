import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { usePetDetailController } from '../controllers/usePetDetailController';
import { PetPhoto } from './components/PetPhoto';
import { colors, spacing } from './theme';

/** VISTA: Detalle de la publicación. */
export function PetDetailView({ petId }: { petId: string }) {
  const { pet, callOwner } = usePetDetailController(petId);

  if (!pet) {
    return (
      <View style={[styles.screen, styles.center]}>
        <Text style={styles.meta}>No existe una publicación con ese id</Text>
      </View>
    );
  }

  return (
    <ScrollView style={styles.screen} contentContainerStyle={styles.content}>
      <PetPhoto uri={pet.image_url} petType={pet.pet_type} iconSize={100} style={styles.photo} />
      <View style={styles.heading}>
        <Text style={styles.name}>{pet.name ?? 'Sin nombre'}</Text>
        <View style={styles.badge}><Text style={styles.badgeText}>Perdido</Text></View>
      </View>
      <Text style={styles.meta}>{[pet.pet_type, pet.breed].filter(Boolean).join(' · ')}</Text>
      <Text style={styles.zone}>📍 {pet.zone}</Text>

      <Pressable
        accessibilityRole="button"
        accessibilityLabel={`Llamar al ${pet.contact_info}`}
        onPress={callOwner}
        style={({ pressed }) => [styles.contact, pressed && styles.pressed]}
      >
        <Text style={styles.contactTitle}>📞 Contactar</Text>
        <Text style={styles.contactNumber}>{pet.contact_info}</Text>
      </Pressable>

      <View style={styles.info}>
        <Text style={styles.infoTitle}>Más información</Text>
        {pet.breed ? <Text style={styles.infoText}>Raza: {pet.breed}</Text> : null}
        <Text style={styles.infoText}>Zona donde se perdió: {pet.zone}</Text>
        <Text style={styles.infoText}>Señas: {pet.color_description}</Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  center: { alignItems: 'center', justifyContent: 'center' },
  content: { padding: spacing.lg, paddingBottom: spacing.xl },
  photo: { height: 250, borderRadius: 18, marginBottom: spacing.lg },
  heading: { flexDirection: 'row', alignItems: 'center', gap: spacing.md },
  name: { color: colors.blue, fontSize: 30, fontWeight: '800', flexShrink: 1 },
  badge: { backgroundColor: colors.primary, borderRadius: 16, paddingHorizontal: spacing.md, paddingVertical: spacing.xs },
  badgeText: { color: colors.primaryText, fontWeight: '700' },
  meta: { color: colors.muted, fontSize: 16, marginTop: spacing.xs },
  zone: { color: colors.blue, fontSize: 16, fontWeight: '700', marginTop: spacing.md },
  contact: { backgroundColor: colors.softBlue, borderRadius: 16, padding: spacing.lg, marginTop: spacing.xl },
  contactTitle: { color: colors.blue, fontSize: 17, fontWeight: '700' },
  contactNumber: { color: colors.blue, fontSize: 21, fontWeight: '700', marginTop: spacing.xs },
  info: { backgroundColor: colors.card, borderRadius: 16, padding: spacing.lg, marginTop: spacing.lg, borderWidth: 1, borderColor: colors.border },
  infoTitle: { color: colors.blue, fontSize: 19, fontWeight: '700', marginBottom: spacing.md },
  infoText: { color: colors.muted, fontSize: 15, marginBottom: spacing.sm },
  pressed: { opacity: 0.7 },
});
