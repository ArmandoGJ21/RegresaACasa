import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { PetsProvider } from '../controllers/PetsContext';
import { colors } from '../views/theme';

// Rutas (Expo Router). Cada archivo en src/app solo conecta una ruta con su Vista.
// El muro no lleva barra superior; el formulario y el detalle solo muestran la flecha de regreso.
export default function RootLayout() {
  return (
    <PetsProvider>
      <StatusBar style="dark" />
      <Stack
        screenOptions={{
          title: '',
          headerTintColor: colors.blue,
          headerShadowVisible: false,
          headerStyle: { backgroundColor: colors.background },
          contentStyle: { backgroundColor: colors.background },
        }}
      >
        <Stack.Screen name="index" options={{ headerShown: false }} />
        <Stack.Screen name="report" />
        <Stack.Screen name="pets/[id]" />
      </Stack>
    </PetsProvider>
  );
}
