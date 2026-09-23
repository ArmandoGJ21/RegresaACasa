import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { colors, spacing } from './theme';

const examples = [
  { name: 'Max', type: 'Perro · Golden Retriever', zone: 'Centro', icon: '🐶' },
  { name: 'Luna', type: 'Gato · Mestiza', zone: 'Pulgas', icon: '🐱' },
];

/** Primer avance visual del muro. Datos y botones de muestra. */
export function FeedView() {
  return (
    <ScrollView style={styles.screen} contentContainerStyle={styles.content}>
      <Text style={styles.brand}>🐾 Regresa a casa</Text>
      <Text style={styles.subtitle}>Mascotas reportadas</Text>
      {examples.map((pet) => (
        <View key={pet.name} style={styles.card}>
          <View style={styles.photo}><Text style={styles.photoIcon}>{pet.icon}</Text></View>
          <View style={styles.cardBody}>
            <Text style={styles.petName}>{pet.name}</Text>
            <Text style={styles.meta}>{pet.type}</Text>
            <Text style={styles.zone}>📍 {pet.zone}</Text>
            <View style={styles.smallButton}><Text style={styles.smallButtonText}>Ver publicación</Text></View>
          </View>
        </View>
      ))}
      <View style={styles.button}><Text style={styles.buttonText}>＋ Publicar mascota</Text></View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  content: { padding: spacing.lg, paddingBottom: spacing.xl },
  brand: { color: colors.blue, fontSize: 27, fontWeight: '800', textAlign: 'center', marginTop: spacing.md },
  subtitle: { color: colors.muted, fontSize: 16, textAlign: 'center', marginBottom: spacing.xl },
  card: { flexDirection: 'row', backgroundColor: colors.card, borderRadius: 18, padding: spacing.md, marginBottom: spacing.lg, borderWidth: 1, borderColor: colors.border },
  photo: { width: 112, height: 132, borderRadius: 14, backgroundColor: colors.softBlue, alignItems: 'center', justifyContent: 'center' },
  photoIcon: { fontSize: 56 },
  cardBody: { flex: 1, paddingLeft: spacing.md, justifyContent: 'center' },
  petName: { color: colors.blue, fontSize: 23, fontWeight: '700' },
  meta: { color: colors.muted, fontSize: 13, marginTop: spacing.xs },
  zone: { color: colors.blue, fontSize: 15, marginTop: spacing.sm },
  smallButton: { backgroundColor: colors.primary, borderRadius: 14, paddingVertical: 9, alignItems: 'center', marginTop: spacing.md },
  smallButtonText: { color: colors.primaryText, fontWeight: '700', fontSize: 13 },
  button: { backgroundColor: colors.primary, borderRadius: 18, minHeight: 50, alignItems: 'center', justifyContent: 'center', marginTop: spacing.sm },
  buttonText: { color: colors.primaryText, fontSize: 16, fontWeight: '700' },
});
