import OpenAI from "openai";
import { openaiApiKey } from './config.js'
import express from 'express';
import fs from "fs";
import path from "path";

const app = express();
app.use(express.json()).post('/generate-speech', async (req, res) => {
  const { modelConfig } = req.body;
  
try {
  validateRequiredParameters(modelConfig);
  await generateSpeechFile(openaiApiKey, modelConfig)
  res.status(200).json({ success: true });
} catch (error) {
  const errorObject = Object.fromEntries(
    Object.getOwnPropertyNames(error).map(key => [key, error[key]])
  );

  res.status(500).json({ success: false, error: errorObject });
}
});

async function generateSpeechFile(apiKey, modelConfig) {
  const openai = new OpenAI({ apiKey });
  const mp3 = await openai.audio.speech.create({
    model: modelConfig.model,
    input: modelConfig.input,
    voice: modelConfig.voice,
    speed: modelConfig.speed,
  });

  const buffer = Buffer.from(await mp3.arrayBuffer());
  await fs.promises.writeFile(modelConfig.outputPath, buffer);
}

function validateRequiredParameters(modelConfig) {
  let error = '';
  if (!openaiApiKey) {
    error += 'OpenAI API key is missing\n';
  }
  if (!modelConfig.outputPath || !path.basename(modelConfig.outputPath).endsWith('.mp3')) {
    error += 'modelConfig.outputPath must include .mp3 filename\n';
  }
  if (!modelConfig.model) {
    error += 'modelConfig.model is missing\n';
  }
  if (!modelConfig.input) {
    error += 'modelConfig.input is missing\n';
  }
  if (!modelConfig.voice) {
    error += 'modelConfig.voice is missing\n';
  }
  if (!modelConfig.speed) {
    error += 'modelConfig.speed is missing\n';
  }
  if (error) {
    throw new Error(error);
  }
}

export default app;