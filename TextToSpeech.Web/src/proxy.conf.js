const PROXY_CONFIG = [
  {
    //todo delete
    context: [
      "/weatherforecast",
    ],
    target: "https://localhost:7057",
    secure: false
  }
]

module.exports = PROXY_CONFIG;
