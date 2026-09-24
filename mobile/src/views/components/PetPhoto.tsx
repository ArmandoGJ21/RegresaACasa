import { Image, StyleSheet, Text, View, type StyleProp, type ViewStyle } from 'react-native';
import { colors } from '../theme';

const ICONS: Record<string, string> = { Perro: '🐶', Gato: '🐱' };

type Props = { uri?: string | null; petType?: string; iconSize?: number; style?: StyleProp<ViewStyle> };

/** Foto de la mascota sobre un fondo con emoji: si la imagen no carga, se ve el emoji. */
export function PetPhoto({ uri, petType = '', iconSize = 56, style }: Props) {
  return (
    <View style={[styles.box, style]}>
      <Text style={{ fontSize: iconSize }}>{ICONS[petType] ?? '🐾'}</Text>
      {uri ? <Image source={{ uri }} style={StyleSheet.absoluteFill} accessibilityIgnoresInvertColors /> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  box: { backgroundColor: colors.softBlue, alignItems: 'center', justifyContent: 'center', overflow: 'hidden' },
});
