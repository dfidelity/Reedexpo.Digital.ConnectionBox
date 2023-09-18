import playwright from 'playwright-aws-lambda';

const initFormat = () => {
  if (!String.prototype.format) {
    String.prototype.format = function () {
      var args = arguments;
      return this.replace(/{(\d+)}/g, function (match, number) {
        return typeof args[number] != 'undefined' ? args[number] : match;
      });
    };
  }
};

const getExhibitorFontSize = {
  stickers: (length) => {
    const fontMap = {
      3: 128,
      4: 120,
      5: 112,
      6: 104,
      7: 96,
      8: 96,
      9: 88,
      10: 80,
    };
    const key = Math.floor(length / 10);
    if (key < 3) {
      return 144;
    } else if (key > 10) {
      return 72;
    }
    return fontMap[key];
  },
  tentcards: (length) => {
    if (length > 99) return 12;
    if (length > 60) return 14;
    if (length > 36) return 16;

    return 20;
  },
};

const generateExhibitorPoster = (template, templateName, QRCode, metaData) => {
  const { exhibitorName } = metaData;
  return template.format(
    exhibitorName,
    QRCode,
    metaData.mainStand,
    metaData.showLogoUrl,
    getExhibitorFontSize[templateName] ? getExhibitorFontSize[templateName](exhibitorName.length) : 20
  );
};

const generateProductPoster = (template, QRCode, metaData) => {
  initFormat();
  return template.format(metaData.productName, QRCode, metaData.exhibitorName, metaData.mainStand);
};

const generatePosterImage = async (posterContent, browserConfig) => {
  console.debug(posterContent);
  let browser = null;
  const { width, height } = browserConfig;
  browser = await playwright.launchChromium();
  const context = await browser.newContext();

  const page = await context.newPage();
  page.setViewportSize({ width, height });
  await page.setContent(posterContent);

  const screenshot = await page.screenshot({ type: 'png' });
  return screenshot;
};

export const generateExhibitorPosters = async (templates, QRCode, metaData) => {
  initFormat();
  const posters = {};
  console.debug('Generating posters from templates...');
  for (const templateObject of templates) {
    const poster = generateExhibitorPoster(templateObject.template, templateObject.name, QRCode, metaData);
    posters[templateObject.name] = await generatePosterImage(poster, templateObject.browserConfig);
  }

  return posters;
};

export const generateProductPosters = (templates, QRCode, metaData) => {
  initFormat();
  const posters = {};
  templates.forEach(async (temp) => {
    const poster = generateProductPoster(temp.template, QRCode, metaData);
    posters[temp.name] = await generatePosterImage(poster);
  });

  return posters;
};
