import { handler } from './createColleqtQRCodes.js';

const testEvent = {
  Records: [
    {
      EventSource: 'aws:sns',
      EventVersion: '1.0',
      EventSubscriptionArn:
        'arn:aws:sns:eu-west-1:915203318988:connectionbox-generate-colleqt-qrcodes-dev:eb63091e-0b01-4f4d-b020-59ab70d74bc4',
      Sns: {
        Type: 'Notification',
        MessageId: '6eb5cd8a-c181-53ea-80a5-90e80c0c55cb',
        TopicArn: 'arn:aws:sns:eu-west-1:915203318988:connectionbox-generate-colleqt-qrcodes-dev',
        Subject: null,
        Message: {
          exhibitorId: '12140235-c27e-4b50-b3ec-5be0c89dfc3d',
          displayName: 'ConnectionBoxTest21',
          eventEditionGbsCode: 'WTM-23',
          showDomain: 'www.wtm.com',
          platformShowId: 'eve-e06331ab-4449-4a48-9322-68ac8e82d759',
          showId: '6a33a235-34d7-430c-8693-1d0d3677df11',
          stands: ['Stand-123'],
          eventType: 'created',
          emperiaEntitlements: 0,
          MessageAttributes: {
            eventDetails: {
              StringValue: 'Exhibitor CreatedEvent with Exhibitor ID: b9f645fd-3051-4e3a-8082-3c6474619992',
              DataType: 'String',
            },
            CorrelationId: { StringValue: '61ef5330-857b-4c12-94c2-ad040d20d40a', DataType: 'String' },
          },
          timestamp: '2023-08-16T16:08:29.4215991Z',
          id: 'ef98036a-7290-4e52-af7e-08a55ea97d4e',
        },
        Timestamp: '2023-08-25T15:39:57.628Z',
        MessageAttributes: {},
      },
    },
  ],
};

//to execute the handler locally
handler(testEvent);
