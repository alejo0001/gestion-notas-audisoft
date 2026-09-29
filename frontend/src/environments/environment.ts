// Configuración de producción (ng build). En local se usa environment.development.ts (npm start).
// __API_URL__ lo reemplaza el pipeline de GitHub Actions con la URL pública del API (variable API_URL).
export const environment = {
  production: true,
  apiUrl: '__API_URL__',
};
