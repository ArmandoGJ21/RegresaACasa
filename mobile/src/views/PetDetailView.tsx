import { Image, KeyboardAvoidingView, Linking, Platform, ScrollView, StyleSheet, Text, View } from 'react-native';
import { usePetDetailController } from '../controllers/usePetDetailController';
import { CommentItem } from './components/CommentItem';
import { FormField } from './components/FormField';
import { PrimaryButton } from './components/PrimaryButton';
import { colors, spacing } from './theme';

/** VISTA: Detalle de la publicación + comentarios. */
export function PetDetailView({ petId }: { petId: string }) {
  const { pet, userName, setUserName, text, setText, sendComment, sending, error } = usePetDetailController(petId);

  if (!pet) {
    return (
      <View style={[styles.screen, styles.center]}>
        <Text style={styles.muted}>No existe una publicación con ese id</Text>
      </View>
    );
  }

  return (
    <KeyboardAvoidingView style={styles.screen} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <Image source={{ uri: pet.image_url }} style={styles.image} />
        <Text style={styles.title}>{pet.name ?? 'Sin nombre'}</Text>
        <Text style={styles.muted}>
          {[pet.pet_type, pet.breed].filter(Boolean).join(' · ')}
        </Text>
        <Text style={styles.detail}>🎨 {pet.color_description}</Text>
        <Text style={styles.detail}>📍 Zona: {pet.zone}</Text>

        <PrimaryButton
          title={`Llamar al ${pet.contact_info}`}
          variant="outline"
          onPress={() => Linking.openURL(`tel:${pet.contact_info}`)}
        />

        <Text style={styles.section}>Comentarios ({pet.comments.length})</Text>
        {pet.comments.length === 0 ? <Text style={styles.muted}>¿La has visto? Deja un comentario.</Text> : null}
        {pet.comments.map((comment, index) => (
          <CommentItem key={`${comment.created_at}-${index}`} comment={comment} />
        ))}

        <View style={styles.form}>
          <FormField label="Tu nombre" value={userName} onChangeText={setUserName} />
          <FormField
            label="Comentario"
            value={text}
            onChangeText={setText}
            multiline
            placeholder="Lo vi cerca del parque"
            style={styles.multiline}
          />
          {error ? <Text style={styles.error}>{error}</Text> : null}
          <PrimaryButton title="Comentar" onPress={sendComment} loading={sending} />
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
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
  section: { fontSize: 18, fontWeight: '700', color: colors.text, marginTop: spacing.xl },
  form: { marginTop: spacing.lg },
  multiline: { minHeight: 80, textAlignVertical: 'top' },
  error: { color: colors.error, marginBottom: spacing.md },
});
