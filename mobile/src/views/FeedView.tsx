import { FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFeedController } from '../controllers/useFeedController';
import { PetCard } from './components/PetCard';
import { PrimaryButton } from './components/PrimaryButton';
import { colors, spacing } from './theme';

/** VISTA: Muro de mascotas perdidas. */
export function FeedView() {
  const { pets, loading, error, onRefresh, openPet, openReport } = useFeedController();

  return (
    <View style={styles.screen}>
      <FlatList
        data={pets}
        keyExtractor={(pet) => pet.id}
        renderItem={({ item }) => <PetCard pet={item} onPress={() => openPet(item.id)} />}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={loading} onRefresh={onRefresh} />}
        ListHeaderComponent={error ? <Text style={styles.error}>{error}</Text> : null}
        ListEmptyComponent={
          loading ? null : <Text style={styles.empty}>Aún no hay reportes. ¡Sé el primero en publicar!</Text>
        }
      />
      <View style={styles.footer}>
        <PrimaryButton title="Reportar mascota perdida" onPress={openReport} />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background },
  list: { padding: spacing.lg },
  error: { color: colors.error, marginBottom: spacing.md },
  empty: { color: colors.muted, textAlign: 'center', marginTop: spacing.xl },
  footer: { padding: spacing.lg, borderTopWidth: 1, borderTopColor: colors.border, backgroundColor: colors.card },
});
