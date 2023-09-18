import { getEventEditionDetails, getShowDetails } from './api/HttpService.js';
import { createExhibitorQRCodePosters, deleteExhibitorQRCodePosters } from './services/QRCodeGenerationService.js';

export const handler = async (event, context) => {
  console.log = function () {}; //to prevent unnecessary logs from third party libraries

  const messageString = event.Records[0].Sns.Message;
  const message = JSON.parse(messageString);
  console.debug(
    `Received exhibitor ${message.eventType} event for ExhibitorID ${message.exhibitorId} in Colleqt QR code generator`
  );

  const showDetails = await getShowDetails(message.platformShowId);
  console.debug(showDetails);
  if (showDetails.length > 0 && !showDetails[0].isColleqtEnabled) {
    console.debug(
      `Colleqt services are not enabled for the show ${message.platformShowId}
      Skipping QR code generation for this show`
    );
    return;
  }

  if (message.status === 'InActive' || message.emperiaEntitlements !== 1) {
    console.debug(
      `The exhibitor ${message.exhibitorId} is inactive or does not have emperia entitlements
      Skipping QR code generation for this exhibitor`
    );
    await deleteExhibitorQRCodePosters({
      exhibitorId: message.exhibitorId,
      showId: message.showId,
    });
    return;
  }

  const eventEditionDetails = await getEventEditionDetails(message.platformShowId);
  console.debug({ eventEditionDetails: JSON.stringify(eventEditionDetails) });
  const { domain, gbsCode, brand } = eventEditionDetails.data.eventEdition;

  if (!domain || !gbsCode) {
    console.debug(
      `The event edition details are not valid. Please make sure the show ${message.platformShowId} is properly set up in royalbox
      Skipping QR code generation for this show`
    );
    return;
  }

  const metaData = {
    eventType: message.eventType,
    exhibitorId: message.exhibitorId,
    exhibitorName: message.displayName,
    showId: message.showId,
    platformShowId: message.platformShowId,
    mainStand: message.stands[0] || '',
    eventEditionGbsCode: gbsCode,
    showLogoUrl: brand.logoUrl,
    domain,
  };

  await createExhibitorQRCodePosters(metaData);
  console.debug('Lambda execution complete');
};
