import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import Backend from 'i18next-http-backend'
import LanguageDetector from 'i18next-browser-languagedetector'

i18n
  .use(Backend)
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    fallbackLng: 'vi',
    load: 'languageOnly',
    defaultNS: 'common',
    ns: ['common', 'auth', 'audit', 'patient', 'scheduling', 'clinical', 'pharmacy', 'consumables', 'billing', 'optical'],
    interpolation: {
      escapeValue: false,
    },
    backend: {
      loadPath: '/locales/{{lng}}/{{ns}}.json',
    },
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
      lookupLocalStorage: 'ganka28-language',
    },
    react: {
      useSuspense: true,
    },
  })

export default i18n
