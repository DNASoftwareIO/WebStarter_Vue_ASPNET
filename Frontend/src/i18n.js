import { createI18n } from 'vue-i18n' 
import en from './locales/en.json'; 

export const locales = {
  en,
};

const i18n = createI18n({
  legacy: false,
  locale: 'en',
  fallbackLocale: 'en', 
  messages: locales,
  globalInjection: true
});

export default i18n