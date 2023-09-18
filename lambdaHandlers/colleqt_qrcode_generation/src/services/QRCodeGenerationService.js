import QRCodeGenerator from '../QRCodeGenerator.js';
import { exhibitorStickerTemplate, productStickerTemplate } from '../shared/templates.js';
import { generateExhibitorPosters, generateProductPosters } from './PosterService.js';
import { getPosterTemplatesFromS3, uploadFileToS3, removeFileFromS3, updateMergeStatusForShow } from './S3Service.js';

export const createExhibitorQRCodePosters = async (metaData) => {
  console.debug(`Initiated QR code poster generation for exhibitor ${metaData.exhibitorId}`);

  const colleqtUrl = generateColleqtUrl(metaData);
  const qrCodeGenerator = new QRCodeGenerator();
  const QRCode = await qrCodeGenerator.generateQRCode(colleqtUrl);

  console.debug(`Fetching exhibitor QR templates for the show: ${metaData.showId}`);
  const templatePath = `colleqt-qr-templates/${metaData.showId}/exhibitors`;
  const templates = await getPosterTemplatesFromS3(templatePath);
  templates.push(exhibitorStickerTemplate);

  const exhibitorPosters = await generateExhibitorPosters(templates, QRCode, metaData);

  console.debug('Uploading exhibitor posters to S3...');
  for (const posterType of Object.keys(exhibitorPosters)) {
    const path = `colleqt-qr-codes/${metaData.showId}/exhibitors/${posterType}/${metaData.exhibitorId}.png`;
    await uploadFileToS3(exhibitorPosters[posterType], path);
  }
  await updateMergeStatusForShow(`colleqt-qr-codes/${metaData.showId}/status.json`);

  console.debug(`QR code poster upload for exhibitor ${metaData.exhibitorId} is complete`);
};

export const deleteExhibitorQRCodePosters = async (metaData) => {
  console.debug(`Initiated QR code poster delete for exhibitor ${metaData.exhibitorId}`);
  const templatePath = `colleqt-qr-templates/${metaData.showId}/exhibitors`;
  const templates = await getPosterTemplatesFromS3(templatePath);
  templates.push(exhibitorStickerTemplate);

  for (const { name } of templates) {
    const path = `colleqt-qr-codes/${metaData.showId}/exhibitors/${name}/${metaData.exhibitorId}.png`;
    console.debug(`deleting exhibitor ${metaData.exhibitorId} ${name} poster from S3...`);
    await removeFileFromS3(path);
    await updateMergeStatusForShow(`colleqt-qr-codes/${metaData.showId}/status.json`);
    console.debug(`${name} poster delete for exhibitor ${metaData.exhibitorId} is complete`);
  }
};

export const createProductQRCodePosters = async (metaData) => {
  console.debug(`Initiated product QR code poster generation for exhibitor ${metaData.exhibitorId}`);

  const qrCodeGenerator = new QRCodeGenerator();

  console.debug(`Fetching product QR templates for the show: ${metaData.showId}`);
  const templatePath = `colleqt-qr-templates/${metaData.showId}/products`;
  const templates = await getPosterTemplatesFromS3(templatePath);
  templates.push(productStickerTemplate);

  for (const product of metaData.products) {
    const productMetaData = {
      ...metaData,
      productId: product.id,
      productName: product.name,
    };
    const colleqtUrl = generateColleqtUrl(productMetaData);
    const QRCode = qrCodeGenerator.generateQRCode(colleqtUrl);

    const productPosters = generateProductPosters(templates, QRCode, productMetaData);

    for (const posterType of Object.keys(productPosters)) {
      const path = `colleqt-qr-codes/${metaData.showId}/products/${posterType}/${productMetaData.productId}.png`;
      await uploadFileToS3(exhibitorPosters[posterType], path);
    }
  }

  console.debug(`Product QR code posters upload for exhibitor ${metaData.exhibitorId} is complete`);
};

const generateColleqtUrl = (metaData) => {
  const { productName, productId, exhibitorName, exhibitorId, domain, eventEditionGbsCode } = metaData;

  return (
    `${domain}/colleqt/app?s=${eventEditionGbsCode}&e_n=${encodeURIComponent(exhibitorName)}&e=${encodeURIComponent(
      exhibitorId
    )}` + (!!productId ? `&p_n=${encodeURIComponent(productName)}&p=${encodeURIComponent(productId)}` : '')
  );
};
