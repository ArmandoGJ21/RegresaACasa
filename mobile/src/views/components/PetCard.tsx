import { Image, Pressable, StyleSheet, Text, View } from 'react-native';
import type { Pet } from '../../models/pet';
import { colors, spacing } from '../theme';

type Props = { pet: Pet; onPress: () => void };

export function PetCard({ pet, onPress }: Props) {
  const title = pet.name ? `${pet.name} · ${pet.pet_type}` : pet.pet_type;
  return (
    <Pressable accessibilityRole="button" onPress={onPress} style={styles.card}>
      <Image source={{ uri: pet.image_url }} style={styles.image} accessibilityLabel={`Foto de ${title}`} />
      <View style={styles.body}>
        <Text style={styles.title}>{title}</Text>
        {pet.breed ? <Text style={styles.meta}>{pet.breed}</Text> : null}
        <Text style={styles.meta}>{pet.color_description}</Text>
        <Text style={styles.zone}>📍 {pet.zone}</Text>
      </View>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.card,
    borderRadius: 12,
    overflow: 'hidden',
    marginBottom: spacing.lg,
    borderWidth: 1,
    borderColor: colors.border,
  },
  image: { width: '100%', aspectRatio: 3 / 2, backgroundColor: colors.border },
  body: { padding: spacing.md, gap: 2 },
  title: { fontSize: 18, fontWeight: '700', color: colors.text },
  meta: { color: colors.muted },
  zone: { color: colors.text, fontWeight: '600', marginTop: spacing.xs },
});
