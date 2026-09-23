import { Image, KeyboardAvoidingView, Platform, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useReportPetController } from '../controllers/useReportPetController';
import { PET_TYPES } from '../models/pet';
import { FormField } from './components/FormField';
import { PrimaryButton } from './components/PrimaryButton';
import { colors, spacing } from './theme';

/** VISTA: Cuestionario para reportar una mascota perdida. */
export function ReportPetView() {
  const { form, setField, image, pickImage, submit, submitting, error } = useReportPetController();

  return (
    <KeyboardAvoidingView style={styles.screen} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <Pressable accessibilityRole="button" onPress={pickImage} style={styles.photo}>
          {image ? (
            <Image source={{ uri: image.uri }} style={styles.photoImage} />
          ) : (
            <Text style={styles.photoHint}>📷 Toca para elegir una foto</Text>
          )}
        </Pressable>

        <Text style={styles.label}>Tipo de mascota</Text>
        <View style={styles.chips}>
          {PET_TYPES.map((type) => (
            <Pressable
              key={type}
              accessibilityRole="radio"
              accessibilityState={{ selected: form.pet_type === type }}
              onPress={() => setField('pet_type', type)}
              style={[styles.chip, form.pet_type === type && styles.chipSelected]}
            >
              <Text style={[styles.chipText, form.pet_type === type && styles.chipTextSelected]}>{type}</Text>
            </Pressable>
          ))}
        </View>

        <FormField label="Nombre" optional value={form.name} onChangeText={(v) => setField('name', v)} />
        <FormField label="Raza" optional value={form.breed} onChangeText={(v) => setField('breed', v)} />
        <FormField
          label="Color / señas particulares"
          value={form.color_description}
          onChangeText={(v) => setField('color_description', v)}
          placeholder="Miel con mancha blanca"
        />
        <FormField label="Zona" value={form.zone} onChangeText={(v) => setField('zone', v)} placeholder="Centro" />
        <FormField
          label="Teléfono de contacto"
          value={form.contact_info}
          onChangeText={(v) => setField('contact_info', v)}
          keyboardType="phone-pad"
          placeholder="4491234567"
        />

        {error ? <Text style={styles.error}>{error}</Text> : null}
        <PrimaryButton title="Publicar reporte" onPress={submit} loading={submitting} />
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  content: { padding: spacing.lg },
  photo: {
    aspectRatio: 3 / 2,
    borderRadius: 12,
    borderWidth: 1,
    borderStyle: 'dashed',
    borderColor: colors.muted,
    alignItems: 'center',
    justifyContent: 'center',
    overflow: 'hidden',
    marginBottom: spacing.lg,
    backgroundColor: colors.card,
  },
  photoImage: { width: '100%', height: '100%' },
  photoHint: { color: colors.muted, fontSize: 16 },
  label: { color: colors.text, fontWeight: '600', marginBottom: spacing.xs },
  chips: { flexDirection: 'row', gap: spacing.sm, marginBottom: spacing.md },
  chip: {
    paddingHorizontal: spacing.lg,
    paddingVertical: spacing.sm,
    borderRadius: 20,
    borderWidth: 1,
    borderColor: colors.border,
    backgroundColor: colors.card,
  },
  chipSelected: { backgroundColor: colors.primary, borderColor: colors.primary },
  chipText: { color: colors.text },
  chipTextSelected: { color: colors.primaryText, fontWeight: '600' },
  error: { color: colors.error, marginBottom: spacing.md },
});
