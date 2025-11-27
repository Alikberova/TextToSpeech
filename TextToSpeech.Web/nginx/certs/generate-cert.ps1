# powershell -ExecutionPolicy Bypass -File ./generate-cert.ps1

& "C:\Program Files\Git\usr\bin\openssl.exe" req -x509 -newkey rsa:2048 -nodes `
  -keyout "./privkey.pem" `
  -out "./fullchain.pem" `
  -days 365 -subj "/CN=localhost"