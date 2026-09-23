import { useState } from 'react';
import { petApi } from '../models/api/petApi';
import { validateComment } from '../models/pet';
import { usePetsStore } from './PetsContext';

/** CONTROLADOR del detalle: muestra una publicación y agrega comentarios. */
export function usePetDetailController(petId: string) {
  const { pets, addComment } = usePetsStore();
  const pet = pets.find((p) => p.id === petId) ?? null;

  const [userName, setUserName] = useState('');
  const [text, setText] = useState('');
  const [sending, setSending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const sendComment = async () => {
    const input = { user_name: userName, text };
    const validationError = validateComment(input);
    if (validationError) {
      setError(validationError);
      return;
    }

    setSending(true);
    setError(null);
    try {
      const comment = await petApi.addComment(petId, { user_name: userName.trim(), text: text.trim() });
      addComment(petId, comment);
      setText('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'No se pudo enviar el comentario');
    } finally {
      setSending(false);
    }
  };

  return { pet, userName, setUserName, text, setText, sendComment, sending, error };
}
