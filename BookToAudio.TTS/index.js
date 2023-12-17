import fs from "fs";
import path from "path";
import OpenAI from "openai";

const openai = new OpenAI( {apiKey: '' });

const speechFile = path.resolve("./speech.mp3");

//todo increase pause after comma

async function main() {
  const mp3 = await openai.audio.speech.create({
    model: "tts-1",
    voice: "alloy",
    input: "",

  });
  console.log(speechFile);
  const buffer = Buffer.from(await mp3.arrayBuffer());
  await fs.promises.writeFile(speechFile, buffer);
}

main();