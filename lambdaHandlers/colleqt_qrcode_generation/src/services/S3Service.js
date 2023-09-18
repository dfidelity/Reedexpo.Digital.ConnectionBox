import {
  S3Client,
  PutObjectCommand,
  GetObjectCommand,
  ListObjectsV2Command,
  HeadObjectCommand,
  DeleteObjectCommand
} from '@aws-sdk/client-s3';
import { config } from '../shared/config.js';

const { environment } = process.env;
const S3Bucket = config[environment].S3Bucket;

export const uploadFileToS3 = async (file, key) => {
  const imageType = 'image/png';

  const client = new S3Client({});
  const putObject = new PutObjectCommand({
    Bucket: S3Bucket,
    Key: key,
    Body: file,
    ContentEncoding: 'base64',
    ContentType: imageType,
  });

  try {
    await client.send(putObject);
  } catch (e) {
    console.debug(`Error while uploading file: ${key} to bucket: ${S3Bucket}`, e);
  }
};

export const writeJsonToS3 = async (content, key) => {
  const client = new S3Client({});
  const putObject = new PutObjectCommand({
    Bucket: S3Bucket,
    Key: key,
    Body: JSON.stringify(content),
    ContentType: 'application/json',
  });

  console.debug('Writing json to S3 bucket');

  try {
    await client.send(putObject);
  } catch (e) {
    console.debug(`Error while writing json file: ${key} to bucket: ${S3Bucket}`, e);
  }
};

export const checkFileExists = async (key) => {
  const client = new S3Client({});
  const headObjectCommand = new HeadObjectCommand({
    Bucket: S3Bucket,
    Key: key,
  });

  console.debug(`sending HTTP head request to ${key}`);

  try {
    const response = await client.send(headObjectCommand);
    return response;
  } catch (error) {
    console.debug(`Error while sending HTTP head request to ${key}`, error);
    return false;
  }
};

export const readFileFromS3 = async (key) => {
  const client = new S3Client({});
  const getObject = new GetObjectCommand({
    Bucket: S3Bucket,
    Key: key,
  });

  console.debug(`Reading ${key} from S3 bucket`);

  try {
    const response = await client.send(getObject);
    return response.Body.transformToString();
  } catch (e) {
    console.debug(`Error while reading file: ${key} from bucket: ${S3Bucket}`, e);
  }
};

export const removeFileFromS3 = async (key) => {
  const client = new S3Client({});

  const file = await checkFileExists(key);

  if (!!file) {
    try {
      const deleteObject = new DeleteObjectCommand({
        Bucket: S3Bucket,
        Key: key
      });
      await client.send(deleteObject);
    } catch (e) {
      console.debug(`Error while deleting file: ${key} from bucket: ${S3Bucket}`, e);
    }
  }
};

export const getPosterTemplatesFromS3 = async (key) => {
  const client = new S3Client({});
  const listCommand = new ListObjectsV2Command({
    Bucket: S3Bucket,
    Prefix: key,
  });
  const templates = [];

  console.debug('Getting templates for show');

  try {
    const response = await client.send(listCommand);
    for (let index in response.Contents) {
      const templatePath = response.Contents[index].Key;
      const template = await readFileFromS3(templatePath);
      templates.push(JSON.parse(template));
    }
  } catch (e) {
    console.debug(`Error while getting templates from bucket: ${S3Bucket}`, e);
    return templates;
  }

  return templates;
};

export const updateMergeStatusForShow = async (path) => {
  try {
    if (!(await checkFileExists(path))) {
      await writeJsonToS3(`{"merge": "yes"}`, path);
    }
  } catch (e) {
    console.debug('Error while updating merge status', e);
  }
};
