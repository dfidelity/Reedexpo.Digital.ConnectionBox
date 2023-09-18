const styles = {
  body: `
    display:flex;
    flex-direction:column;
    align-items:center;
    justify-content:center
  `
    .replace(/\n|\s\s+/gm, '')
    .trim(),
  exhibitorQR: {
    headerText: `
      font-family:Helvetica Neue, system-ui, -apple-system, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, 'Noto Sans',
            'Liberation Sans', sans-serif;
      font-size:100px;
      width:90%;
      text-align:center;
      font-weight:450;
      margin-block:40px
    `
      .replace(/\n|\s\s+/gm, '')
      .trim(),
    exhibitorName: `
      font-family: Helvetica Neue, system-ui, -apple-system, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, 'Noto Sans',
            'Liberation Sans', sans-serif;
      font-size:{4}px;
      display:inline-block;
      width:95%;
      height:8%;
      text-align:center;
      margin-block-start:20px;
      margin-block-end:120px;
      word-wrap:break-word
    `
      .replace(/\n|\s\s+/gm, '')
      .trim(),
    mainStand: `
      font-family: Helvetica Neue, system-ui, -apple-system, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, 'Noto Sans',
            'Liberation Sans', sans-serif;
      font-size:80px;
      font-weight:450;
      margin-block:120px
    `
      .replace(/\n|\s\s+/gm, '')
      .trim(),
  },
};

export const exhibitorStickerTemplate = {
  name: 'stickers',
  template: `
    <html>
      <head>
        <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Helvetica+Neue">
      </head>
      <body width="100vw" height="100vh" style="${styles.body}">
          <p style="${styles.exhibitorQR.headerText}">information &amp connect with</p>
          <h3 style="${styles.exhibitorQR.exhibitorName}">{0}</h3>
          <img src="{1}" alt="QR Code"/>
          <p style="${styles.exhibitorQR.mainStand}">{2}</p>
      </body>
    </html>
  `,
  browserConfig: {
    width: 2480,
    height: 3508,
  },
};

export const productStickerTemplate = {
  name: 'stickers',
  template: `
    <html>
      <body width="100vw" height="100vh" style="${styles.body}">
          <h3>{0}</h3>
          <img src="{1}" alt="QR Code">
          <p>{2}</p>
          <p>{3}</p>
      </body>
    </html>
  `,
  browserConfig: {
    width: 2480,
    height: 3508,
  },
};
