import { Image, KeyboardAvoidingView, Platform, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useReportPetController } from '../controllers/useReportPetController';
import { PET_TYPES } from '../models/pet';
import { FormField } from './components/FormField';
import { PrimaryButton } from './components/PrimaryButton';
import { colors, spacing } from './theme';

/** VISTA: Formulario para publicar una mascota perdida. */
export function ReportPetView() {
  const { form, setField, image, pickImage, submit, submitting, error } = useReportPetController();

  return (
    <KeyboardAvoidingView style={styles.screen} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <Text style={styles.title}>Publicar mascota perdida</Text>

        <Pressable accessibilityRole="button" accessibilityLabel="Elegir foto" onPress={pickImage} style={styles.photo}>
          {image ? (
            <Image source={{ uri: image.uri }} style={styles.photoImage} />
          ) : (
            <>
              <Text style={styles.camera}>📷</Text>
              <Text style={styles.photoTitle}>Sube una foto de tu mascota</Text>
              <Text style={styles.hint}>Toca para seleccionar una imagen</Text>
            </>
          )}
        </Pressable>

        <FormField label="Nombre de la mascota" optional placeholder="Ej. Max" value={form.name} onChangeText={(v) => setField('name', v)} />

        <Text style={styles.label}>Tipo de mascota</Text>
        <View style={styles.chips}>
          {PET_TYPES.map((type) => {
            const selected = form.pet_type === type;
            return (
              <Pressable
                key={type}
                accessibilityRole="radio"
                accessibilityState={{ selected }}
                onPress={() => setField('pet_type', type)}
                style={[styles.chip, selected && styles.chipSelected]}
              >
                <Text style={[styles.chipText, selected && styles.chipTextSelected]}>{type}</Text>
              </Pressable>
            );
          })}
        </View>

        <FormField label="Raza" optional placeholder="Ej. Golden Retriever" value={form.breed} onChangeText={(v) => setField('breed', v)} />
        <FormField
          label="Color y señas"
          placeholder="Ej. Dorado, collar azul"
          value={form.color_description}
          onChangeText={(v) => setField('color_description', v)}
        />
        <FormField label="Zona donde se perdió" placeholder="Ej. Centro" value={form.zone} onChangeText={(v) => setField('zone', v)} />
        <FormField
          label="Información de contacto"
          placeholder="Ej. 555 123 4567"
          keyboardType="phone-pad"
          value={form.contact_info}
          onChangeText={(v) => setField('contact_info', v)}
        />

        {error ? <Text style={styles.error}>{error}</Text> : null}
        <PrimaryButton title="Publicar" onPress={submit} loading={submitting} />
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  content: { padding: spacing.lg, paddingBottom: spacing.xl },
  title: { color: colors.blue, fontSize: 23, fontWeight: '700', textAlign: 'center', marginBottom: spacing.lg },
  photo: { backgroundColor: colors.card, borderColor: colors.lightBlue, borderWidth: 2, borderStyle: 'dashed', borderRadius: 16, minHeight: 160, alignItems: 'center', justifyContent: 'center', marginBottom: spacing.lg, overflow: 'hidden' },
  photoImage: { width: '100%', aspectRatio: 3 / 2 },
  camera: { fontSize: 34, marginBottom: spacing.xs },
  photoTitle: { color: colors.blue, fontSize: 16, fontWeight: '700' },
  hint: { color: colors.muted, fontSize: 13, marginTop: spacing.xs },
  label: { color: colors.blue, fontWeight: '700', fontSize: 14, marginBottom: spacing.xs },
  chips: { flexDirection: 'row', gap: spacing.sm, marginBottom: spacing.md },
  chip: { borderWidth: 1, borderColor: colors.lightBlue, backgroundColor: colors.card, borderRadius: 11, minHeight: 44, paddingHorizontal: spacing.lg, justifyContent: 'center' },
  chipSelected: { backgroundColor: colors.blue, borderColor: colors.blue },
  chipText: { color: colors.blue, fontSize: 15 },
  chipTextSelected: { color: colors.primaryText, fontWeight: '700' },
  error: { color: colors.error, marginBottom: spacing.sm, textAlign: 'center' },
});
