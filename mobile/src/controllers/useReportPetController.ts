import * as ImagePicker from 'expo-image-picker';
import { useRouter } from 'expo-router';
import { useState } from 'react';
import { uploadPetImage } from '../models/api/imageUploadApi';
import { petApi } from '../models/api/petApi';
import { PET_TYPES, validatePetForm } from '../models/pet';
import { usePetsStore } from './PetsContext';

type PickedImage = { uri: string; mimeType: string };

const EMPTY_FORM = {
  pet_type: PET_TYPES[0] as string,
  name: '',
  breed: '',
  color_description: '',
  zone: '',
  contact_info: '',
};

export type ReportForm = typeof EMPTY_FORM;

/** CONTROLADOR del Cuestionario: foto -> Azure, luego POST /api/v1/pets. */
export function useReportPetController() {
  const router = useRouter();
  const { addPet } = usePetsStore();
  const [form, setForm] = useState<ReportForm>(EMPTY_FORM);
  const [image, setImage] = useState<PickedImage | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const setField = (field: keyof ReportForm, value: string) =>
    setForm((current) => ({ ...current, [field]: value }));

  const pickImage = async () => {
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ['images'],
      quality: 0.7,
      allowsEditing: true,
    });
    if (!result.canceled) {
      const asset = result.assets[0];
      setImage({ uri: asset.uri, mimeType: asset.mimeType ?? 'image/jpeg' });
    }
  };

  const submit = async () => {
    const validationError = validatePetForm(form, image !== null);
    if (validationError) {
      setError(validationError);
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      const imageUrl = await uploadPetImage(image!.uri, image!.mimeType);
      const pet = await petApi.create({
        ...form,
        name: form.name.trim() || undefined,
        breed: form.breed.trim() || undefined,
        image_url: imageUrl,
      });
      addPet(pet);
      router.back();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'No se pudo publicar el reporte');
    } finally {
      setSubmitting(false);
    }
  };

  return { form, setField, image, pickImage, submit, submitting, error };
}
