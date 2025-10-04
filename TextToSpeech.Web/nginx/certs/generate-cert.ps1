& "C:\Program Files\Git\usr\bin\openssl.exe" req -x509 -newkey rsa:2048 -nodes `
  -keyout "$HOME\source\repos\TextToSpeech\TextToSpeech.Web\nginx\certs\privkey.pem" `
  -out "$HOME\source\repos\TextToSpeech\TextToSpeech.Web\nginx\certs\fullchain.pem" `
  -days 365 -subj "/CN=localhost"
