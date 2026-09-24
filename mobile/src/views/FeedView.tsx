import { FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFeedController } from '../controllers/useFeedController';
import { PetCard } from './components/PetCard';
import { PrimaryButton } from './components/PrimaryButton';
import { colors, spacing } from './theme';

/** VISTA: Muro de mascotas reportadas. */
export function FeedView() {
  const { pets, loading, error, onRefresh, openPet, openReport } = useFeedController();

  return (
    <View style={styles.screen}>
      <FlatList
        data={pets}
        keyExtractor={(pet) => pet.id}
        renderItem={({ item }) => <PetCard pet={item} onPress={() => openPet(item.id)} />}
        contentContainerStyle={styles.content}
        refreshControl={<RefreshControl refreshing={loading} onRefresh={onRefresh} tintColor={colors.blue} />}
        ListHeaderComponent={
          <>
            <Text style={styles.brand}>🐾 Regresa a casa</Text>
            <Text style={styles.subtitle}>Mascotas reportadas</Text>
            {error ? <Text style={styles.error}>{error}</Text> : null}
          </>
        }
        ListEmptyComponent={
          loading || error ? null : <Text style={styles.empty}>Aún no hay reportes. ¡Sé el primero en publicar!</Text>
        }
      />
      <View style={styles.footer}>
        <PrimaryButton title="＋ Publicar mascota" onPress={openReport} />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  content: { padding: spacing.lg, paddingBottom: spacing.xl },
  brand: { color: colors.blue, fontSize: 27, fontWeight: '800', textAlign: 'center', marginTop: spacing.md },
  subtitle: { color: colors.muted, fontSize: 16, textAlign: 'center', marginBottom: spacing.xl },
  error: { color: colors.error, textAlign: 'center', marginBottom: spacing.lg },
  empty: { color: colors.muted, textAlign: 'center', marginTop: spacing.xl },
  footer: { paddingHorizontal: spacing.lg, paddingBottom: spacing.lg, backgroundColor: colors.background },
});
