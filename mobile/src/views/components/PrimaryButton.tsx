import { ActivityIndicator, Pressable, StyleSheet, Text } from 'react-native';
import { colors, spacing } from '../theme';

type Props = { title: string; onPress: () => void; loading?: boolean; variant?: 'solid' | 'outline' };

export function PrimaryButton({ title, onPress, loading = false, variant = 'solid' }: Props) {
  const outline = variant === 'outline';
  return (
    <Pressable
      accessibilityRole="button"
      onPress={onPress}
      disabled={loading}
      style={({ pressed }) => [styles.button, outline && styles.outline, (pressed || loading) && styles.pressed]}
    >
      {loading ? (
        <ActivityIndicator color={outline ? colors.primary : colors.primaryText} />
      ) : (
        <Text style={[styles.text, outline && styles.outlineText]}>{title}</Text>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  button: {
    backgroundColor: colors.primary,
    borderRadius: 10,
    paddingVertical: spacing.md,
    alignItems: 'center',
  },
  outline: { backgroundColor: 'transparent', borderWidth: 1, borderColor: colors.primary },
  pressed: { opacity: 0.7 },
  text: { color: colors.primaryText, fontWeight: '600', fontSize: 16 },
  outlineText: { color: colors.primary },
});
