export const config = {
  dev: {
    baseApiUrl: 'https://reedexpopreprod-dev.apigee.net',
    S3Bucket: 'connectionbox-dev-connectionboxuploadedfiless3buc-vopzoyw7s18l',
  },
  show: {
    baseApiUrl: 'https://reedexpopreprod-demo.apigee.net',
    S3Bucket: 'connectionbox-show-connectionboxuploadedfiless3bu-pyp10l6t35vr',
  },
  preprod: {
    baseApiUrl: 'https://api.rxweb-pre.com',
    S3Bucket: 'connectionbox-preprod-connectionboxuploadedfiless-1qlt7r6ocoynm',
  },
  prod: {
    baseApiUrl: 'https://api.reedexpo.com',
    S3Bucket: 'connectionbox-prod-connectionboxuploadedfiless3bu-1vq4xanaw3mm2',
  },
  qrSize: 2320,
};
