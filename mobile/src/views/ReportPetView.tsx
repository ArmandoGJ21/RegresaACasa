import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { colors, spacing } from './theme';

const fields = [
  ['Nombre de la mascota', 'Ej. Max'],
  ['Tipo de mascota', 'Selecciona'],
  ['Raza', 'Ej. Golden Retriever'],
  ['Color y señas', 'Ej. Dorado, collar azul'],
  ['Zona donde se perdió', 'Ej. Centro'],
  ['Información de contacto', 'Ej. 555 123 4567'],
];

/** Primer avance visual del formulario. Aún no permite escribir ni publicar. */
export function ReportPetView() {
  return (
    <ScrollView style={styles.screen} contentContainerStyle={styles.content}>
      <Text style={styles.title}>Publicar mascota perdida</Text>
      <View style={styles.photo}>
        <Text style={styles.camera}>📷</Text>
        <Text style={styles.photoTitle}>Sube una foto de tu mascota</Text>
        <Text style={styles.hint}>Toca para seleccionar una imagen</Text>
      </View>
      {fields.map(([label, placeholder]) => (
        <View key={label} style={styles.field}>
          <Text style={styles.label}>{label}</Text>
          <View style={styles.input}><Text style={styles.placeholder}>{placeholder}</Text></View>
        </View>
      ))}
      <View style={styles.button}><Text style={styles.buttonText}>Publicar</Text></View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  content: { padding: spacing.lg, paddingBottom: spacing.xl },
  title: { color: colors.blue, fontSize: 23, fontWeight: '700', textAlign: 'center', marginBottom: spacing.lg },
  photo: { backgroundColor: colors.card, borderColor: colors.lightBlue, borderWidth: 2, borderStyle: 'dashed', borderRadius: 16, minHeight: 160, alignItems: 'center', justifyContent: 'center', marginBottom: spacing.lg },
  camera: { fontSize: 34, marginBottom: spacing.xs },
  photoTitle: { color: colors.blue, fontSize: 16, fontWeight: '700' },
  hint: { color: colors.muted, fontSize: 13, marginTop: spacing.xs },
  field: { marginBottom: spacing.md },
  label: { color: colors.blue, fontWeight: '700', fontSize: 14, marginBottom: spacing.xs },
  input: { backgroundColor: colors.card, borderColor: colors.lightBlue, borderWidth: 1, borderRadius: 11, minHeight: 48, paddingHorizontal: spacing.md, justifyContent: 'center' },
  placeholder: { color: colors.muted, fontSize: 15 },
  button: { backgroundColor: colors.primary, minHeight: 50, borderRadius: 18, alignItems: 'center', justifyContent: 'center', marginTop: spacing.md },
  buttonText: { color: colors.primaryText, fontSize: 17, fontWeight: '700' },
});
