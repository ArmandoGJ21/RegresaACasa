import { StyleSheet, Text, TextInput, View, type TextInputProps } from 'react-native';
import { colors, spacing } from '../theme';

type Props = TextInputProps & { label: string; optional?: boolean };

export function FormField({ label, optional, style, ...inputProps }: Props) {
  return (
    <View style={styles.field}>
      <Text style={styles.label}>
        {label}
        {optional ? <Text style={styles.optional}> (opcional)</Text> : null}
      </Text>
      <TextInput placeholderTextColor={colors.muted} style={[styles.input, style]} {...inputProps} />
    </View>
  );
}

const styles = StyleSheet.create({
  field: { marginBottom: spacing.md },
  label: { color: colors.blue, fontWeight: '700', fontSize: 14, marginBottom: spacing.xs },
  optional: { color: colors.muted, fontWeight: '400' },
  input: {
    backgroundColor: colors.card,
    borderColor: colors.lightBlue,
    borderWidth: 1,
    borderRadius: 11,
    minHeight: 48,
    paddingHorizontal: spacing.md,
    fontSize: 15,
    color: colors.text,
  },
});
