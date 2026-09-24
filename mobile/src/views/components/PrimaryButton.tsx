import { ActivityIndicator, Pressable, StyleSheet, Text } from 'react-native';
import { colors, spacing } from '../theme';

type Props = { title: string; onPress: () => void; loading?: boolean };

export function PrimaryButton({ title, onPress, loading = false }: Props) {
  return (
    <Pressable
      accessibilityRole="button"
      onPress={onPress}
      disabled={loading}
      style={({ pressed }) => [styles.button, (pressed || loading) && styles.pressed]}
    >
      {loading ? <ActivityIndicator color={colors.primaryText} /> : <Text style={styles.text}>{title}</Text>}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  button: {
    backgroundColor: colors.primary,
    borderRadius: 18,
    minHeight: 50,
    alignItems: 'center',
    justifyContent: 'center',
    marginTop: spacing.sm,
  },
  pressed: { opacity: 0.7 },
  text: { color: colors.primaryText, fontSize: 16, fontWeight: '700' },
});
