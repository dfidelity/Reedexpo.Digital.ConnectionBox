import axios from 'axios';
import { checkFileExists, readFileFromS3, writeJsonToS3 } from '../services/S3Service.js';

const HttpClient = () => {
  const { apiKey } = process.env;

  const axiosClient = axios.create({
    headers: {
      'x-clientid': apiKey,
    },
  });

  const isCacheValid = async (path) => {
    const response = await checkFileExists(path);
    console.debug({ response });
    if (response) {
      return Math.abs(new Date().valueOf() - response.LastModified.valueOf()) / 60000 < 30;
    }
    return false;
  };

  const cacheAndGetResponse = async (path, callback) => {
    if (await isCacheValid(path)) {
      console.debug('Reading cached response from S3');
      const response = await readFileFromS3(path);
      return JSON.parse(response);
    }
    const response = await callback();
    if(response.status === 200)
      await writeJsonToS3(response.data, path);
    return response.data;
  };

  return {
    get: (url, cachePath, config) => cacheAndGetResponse(cachePath, () => axiosClient.get(url, config)),
    post: (url, cachePath, config) => cacheAndGetResponse(cachePath, () => axiosClient.post(url, config)),
  };
};

export default HttpClient;
