import fs from "fs";
import path from "path";
import express from 'express';
import { fileURLToPath } from 'url';
import { dirname } from 'path';

//http://localhost:3000/download/somefile.mp3

const app = express();
const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

app.get('/download/:filename', (req, res) => {
  const filename = req.params.filename;
  const filePath = path.join(__dirname, 'mp3_files', filename);
  
  if (!fs.existsSync(filePath))
  {
    res.status(404).send('File not found');
    return;
  }

  res.download(filePath, (err) => {
    if (err) {
      console.error('Error sending file:', err);
      res.status(500).send('Internal Server Error');
    }
  });
});

export default app;