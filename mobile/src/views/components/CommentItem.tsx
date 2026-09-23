import { StyleSheet, Text, View } from 'react-native';
import type { PetComment } from '../../models/pet';
import { colors, spacing } from '../theme';

export function CommentItem({ comment }: { comment: PetComment }) {
  const date = new Date(comment.created_at).toLocaleString('es-MX', { dateStyle: 'short', timeStyle: 'short' });
  return (
    <View style={styles.item}>
      <View style={styles.header}>
        <Text style={styles.author}>{comment.user_name}</Text>
        <Text style={styles.date}>{date}</Text>
      </View>
      <Text style={styles.text}>{comment.text}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  item: { paddingVertical: spacing.sm, borderBottomWidth: 1, borderBottomColor: colors.border },
  header: { flexDirection: 'row', justifyContent: 'space-between' },
  author: { fontWeight: '600', color: colors.text },
  date: { color: colors.muted, fontSize: 12 },
  text: { color: colors.text, marginTop: 2 },
});
