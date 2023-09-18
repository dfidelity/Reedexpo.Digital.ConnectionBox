import axios from 'axios';
import { config } from '../shared/config.js';
import HttpClient from './HttpClient.js';

const { environment, apiKey } = process.env;
const baseApiUrl = config[environment].baseApiUrl;
const httpClient = HttpClient();

export const getEventEditionDetails = async (eventEditionId) => {
  const cachePath = `caches/qr-code-generator/eventEditionResponse/${eventEditionId}/response.json`;

  try {
    return await httpClient.post(`${baseApiUrl}/graphql/`, cachePath, {
      query: `query eventEdition($eventEditionId: String!) { eventEdition(eventEditionId: $eventEditionId) { domain, gbsCode, brand { logoUrl } } }`,
      variables: { eventEditionId },
    });
  } catch (e) {
    console.debug(`Error while fetching event edition details for platformShowId: ${eventEditionId}`, e);
  }
};

export const getShowDetails = async (platformShowId) => {
  const cachePath = `caches/qr-code-generator/showResponse/${platformShowId}/response.json`;
  try {
    return await httpClient.get(`${baseApiUrl}/api/v1/lead-capture-shows/?platformShowId=${platformShowId}`, cachePath);
  } catch (e) {
    console.debug(`Error while fetching show details for platformShowId: ${platformShowId}`, e);
  }
};

export const getExhibitorAndProductDetails = async (showId, platformExhibitorId) => {
  const tokenStr = process.env.AUTH_TOKEN;
  return await axios.get(`${baseApiUrl}/v1/exhibitors?showId=${showId}&platformExhibitorId=${platformExhibitorId}`, {
    headers: {
      ...httpConfig.headers,
      Authorization: `Bearer ${tokenStr}`,
    },
  });
};
