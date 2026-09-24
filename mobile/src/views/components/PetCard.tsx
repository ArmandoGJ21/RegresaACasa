import { Pressable, StyleSheet, Text, View } from 'react-native';
import type { Pet } from '../../models/pet';
import { colors, spacing } from '../theme';
import { PetPhoto } from './PetPhoto';

type Props = { pet: Pet; onPress: () => void };

/** Tarjeta del muro (diseño de Deisy). */
export function PetCard({ pet, onPress }: Props) {
  const title = pet.name ?? pet.pet_type;
  return (
    <View style={styles.card}>
      <PetPhoto uri={pet.image_url} petType={pet.pet_type} style={styles.photo} />
      <View style={styles.cardBody}>
        <Text style={styles.petName} numberOfLines={1}>{title}</Text>
        <Text style={styles.meta} numberOfLines={1}>{[pet.pet_type, pet.breed].filter(Boolean).join(' · ')}</Text>
        <Text style={styles.zone} numberOfLines={1}>📍 {pet.zone}</Text>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel={`Ver publicación de ${title}`}
          onPress={onPress}
          style={({ pressed }) => [styles.smallButton, pressed && styles.pressed]}
        >
          <Text style={styles.smallButtonText}>Ver publicación</Text>
        </Pressable>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  card: { flexDirection: 'row', backgroundColor: colors.card, borderRadius: 18, padding: spacing.md, marginBottom: spacing.lg, borderWidth: 1, borderColor: colors.border },
  photo: { width: 112, height: 132, borderRadius: 14 },
  cardBody: { flex: 1, paddingLeft: spacing.md, justifyContent: 'center' },
  petName: { color: colors.blue, fontSize: 23, fontWeight: '700' },
  meta: { color: colors.muted, fontSize: 13, marginTop: spacing.xs },
  zone: { color: colors.blue, fontSize: 15, marginTop: spacing.sm },
  smallButton: { backgroundColor: colors.primary, borderRadius: 14, paddingVertical: 9, alignItems: 'center', marginTop: spacing.md },
  smallButtonText: { color: colors.primaryText, fontWeight: '700', fontSize: 13 },
  pressed: { opacity: 0.7 },
});
