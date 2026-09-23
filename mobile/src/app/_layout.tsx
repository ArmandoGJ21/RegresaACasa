import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { colors } from '../views/theme';

// En este avance las rutas muestran únicamente las pantallas estáticas.
export default function RootLayout() {
  return (
    <>
      <StatusBar style="dark" />
      <Stack
        screenOptions={{
          headerShown: false,
          contentStyle: { backgroundColor: colors.background },
        }}
      >
        <Stack.Screen name="index" />
        <Stack.Screen name="report" />
        <Stack.Screen name="pets/[id]" />
      </Stack>
    </>
  );
}
