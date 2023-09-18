import { QRCodeStyling } from 'qr-code-styling-node/lib/qr-code-styling.common.js';
import canvas from 'canvas';
import { fileURLToPath } from 'url';
import { JSDOM } from 'jsdom';
import { config } from './shared/config.js';

class QRCodeGenerator {
  options = {
    jsdom: JSDOM,
    type: 'svg',
    nodeCanvas: canvas,
    width: config.qrSize,
    height: config.qrSize,
    image: fileURLToPath(new URL('../assets/logo.svg', import.meta.url)),
    dotsOptions: {
      color: '#000',
      type: 'square',
    },
    cornersSquareOptions: {
      color: '#000',
      type: 'extra-rounded',
    },
    backgroundOptions: {
      color: '#fff',
    },
    imageOptions: {
      saveAsBlob: true,
      hideBackgroundDots: false,
    },
  };

  async generateQRCode(data) {
    const qrCodeStyling = new QRCodeStyling({ ...this.options, data });
    const QRCode = await qrCodeStyling.getRawData('png');
    return `data:image/png;base64,${QRCode.toString('base64')}`;
  }
}

export default QRCodeGenerator;
