import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { PetsProvider } from '../controllers/PetsContext';
import { colors } from '../views/theme';

// Rutas (Expo Router). Cada archivo en src/app solo conecta una ruta con su Vista.
export default function RootLayout() {
  return (
    <PetsProvider>
      <StatusBar style="dark" />
      <Stack
        screenOptions={{
          headerTintColor: colors.primary,
          headerTitleStyle: { color: colors.text },
          contentStyle: { backgroundColor: colors.background },
        }}
      >
        <Stack.Screen name="index" options={{ title: 'Regresa a Casa' }} />
        <Stack.Screen name="report" options={{ title: 'Reportar mascota', presentation: 'modal' }} />
        <Stack.Screen name="pets/[id]" options={{ title: 'Publicación' }} />
      </Stack>
    </PetsProvider>
  );
}
