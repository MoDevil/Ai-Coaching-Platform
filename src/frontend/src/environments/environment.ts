export const environment = {
  production: false,
  // Relative so that `ng serve` routes API calls through proxy.conf.json. This keeps the browser
  // on a single origin during development, so CORS is never exercised locally and cannot mask a
  // missing CORS rule in production.
  apiUrl: '/api'
};
