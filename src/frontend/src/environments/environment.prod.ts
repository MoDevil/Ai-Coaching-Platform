export const environment = {
  production: true,
  // Relative on purpose: the production bundle is served from the same origin as the API behind
  // the reverse proxy, so the base URL is deployment specific and must not be baked in at build
  // time. `ng build --configuration production` swaps this file in via the fileReplacements entry
  // in angular.json; without that entry this file would never be used and the development URL
  // would be shipped.
  apiUrl: '/api'
};
