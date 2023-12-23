import { port, host } from './config.js'
import fileServiceApp from './file-service.js';
import openaiService from './openai-service.js';
import express from 'express';

async function main() {
  const mainRouter = express.Router();

  mainRouter.use(openaiService);
  mainRouter.use(fileServiceApp);

  const app = express();
  
  app.use(mainRouter);

  app.listen(port, host, () => {
    console.log(`Server is running on http://${host}:${port}`);
  });
}

main();