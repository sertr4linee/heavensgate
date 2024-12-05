import axios from "axios";
import { IdentityService } from "./identity-service";

const serverUrl = process.env.NEXT_PUBLIC_SERVER_URL;

const api = axios.create({
  baseURL: serverUrl,
  withCredentials: true,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
    'X-Requested-With': 'XMLHttpRequest'
  }
});

// Ajout d'un intercepteur pour la mise en cache
const cache = new Map();

api.interceptors.request.use((config) => {
  // Ne pas mettre en cache les requêtes POST/PUT/DELETE
  if (config.method?.toLowerCase() !== 'get') {
    return config;
  }

  const cacheKey = `${config.method}_${config.url}`;
  const cachedResponse = cache.get(cacheKey);

  if (cachedResponse) {
    const { data, timestamp } = cachedResponse;
    // Cache valide pendant 5 minutes
    if (Date.now() - timestamp < 5 * 60 * 1000) {
      return Promise.resolve({ ...config, data });
    }
    cache.delete(cacheKey);
  }

  return config;
});

api.interceptors.response.use(
  (response) => {
    if (response.config.method?.toLowerCase() === 'get') {
      const cacheKey = `${response.config.method}_${response.config.url}`;
      cache.set(cacheKey, {
        data: response.data,
        timestamp: Date.now()
      });
    }
    return response;
  },
  (error) => {
    console.error(`❌ API Error: ${error.message}`);
    return Promise.reject(error);
  }
);

const identityService = new IdentityService(api);

export { api, identityService };
