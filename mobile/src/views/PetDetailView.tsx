import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { colors, spacing } from './theme';

/** Primer avance visual del detalle. El identificador se usará más adelante. */
export function PetDetailView({ petId: _petId }: { petId: string }) {
  return (
    <ScrollView style={styles.screen} contentContainerStyle={styles.content}>
      <View style={styles.photo}><Text style={styles.photoIcon}>🐶</Text></View>
      <View style={styles.heading}>
        <Text style={styles.name}>Max</Text>
        <View style={styles.badge}><Text style={styles.badgeText}>Perdido</Text></View>
      </View>
      <Text style={styles.meta}>Perro · Golden Retriever</Text>
      <Text style={styles.zone}>📍 Centro</Text>
      <Text style={styles.description}>Max se perdió cerca del mercado central. Lleva un collar azul.</Text>
      <View style={styles.contact}>
        <Text style={styles.contactTitle}>📞 Contactar</Text>
        <Text style={styles.contactNumber}>555 123 4567</Text>
      </View>
      <View style={styles.info}>
        <Text style={styles.infoTitle}>Más información</Text>
        <Text style={styles.infoText}>Raza: Golden Retriever</Text>
        <Text style={styles.infoText}>Zona donde se perdió: Centro</Text>
        <Text style={styles.infoText}>Señas: collar azul</Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  content: { padding: spacing.lg, paddingBottom: spacing.xl },
  photo: { height: 250, borderRadius: 18, backgroundColor: colors.softBlue, alignItems: 'center', justifyContent: 'center', marginBottom: spacing.lg },
  photoIcon: { fontSize: 100 },
  heading: { flexDirection: 'row', alignItems: 'center', gap: spacing.md },
  name: { color: colors.blue, fontSize: 30, fontWeight: '800' },
  badge: { backgroundColor: colors.primary, borderRadius: 16, paddingHorizontal: spacing.md, paddingVertical: spacing.xs },
  badgeText: { color: colors.primaryText, fontWeight: '700' },
  meta: { color: colors.muted, fontSize: 16, marginTop: spacing.xs },
  zone: { color: colors.blue, fontSize: 16, fontWeight: '700', marginTop: spacing.md },
  description: { color: colors.muted, fontSize: 16, lineHeight: 24, marginTop: spacing.lg },
  contact: { backgroundColor: colors.softBlue, borderRadius: 16, padding: spacing.lg, marginTop: spacing.xl },
  contactTitle: { color: colors.blue, fontSize: 17, fontWeight: '700' },
  contactNumber: { color: colors.blue, fontSize: 21, fontWeight: '700', marginTop: spacing.xs },
  info: { backgroundColor: colors.card, borderRadius: 16, padding: spacing.lg, marginTop: spacing.lg, borderWidth: 1, borderColor: colors.border },
  infoTitle: { color: colors.blue, fontSize: 19, fontWeight: '700', marginBottom: spacing.md },
  infoText: { color: colors.muted, fontSize: 15, marginBottom: spacing.sm },
});
