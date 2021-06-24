import * as WebSocket from 'websocket';
import * as http from 'http';
import * as readline from 'readline';

const wsConnections = new Set<WebSocket.connection>();

async function main()
{
  const server = await startServer();

  const messageLen = 0;   // Tweak this to get a different stack trace when it crashes.
                          // It'll still crash either way, but a message length
                          // of zero will give us the same results we were seeing
                          // in the lab.
  const pingData = Buffer.alloc(messageLen);
  pingData.fill('a', 0, messageLen);

  while (true)
  {
    await delayAsync(1);
    for (let i = 0; i < 10; i++)
      pingAll(pingData);
  }
}
main();

function delayAsync(milliseconds: number): Promise<void>
{
  return new Promise(resolve => setTimeout(resolve, milliseconds));
}

function pingAll(data: Buffer)
{
  wsConnections.forEach(conn => {
    conn.ping(data);
  });
}

function startServer(): Promise<WebSocket.server>
{
  const port = 22475;

  const httpServer = http.createServer((req, res) =>
  {
    res.writeHead(200);
    res.end();
  });

  const wsServer = new WebSocket.server({
    httpServer: httpServer,
    autoAcceptConnections: true
  });

  wsServer.on('connect', connection =>
  {
    // Keep track of all the active connections in a set, so we can
    // ping them later.
    wsConnections.add(connection);
    connection.on('close', (code, desc) =>
    {
      wsConnections.delete(connection);
    });

    // Count the number of pongs.
    let pongCount = 0;
    connection.on('pong', req =>
    {
      console.log('Received pong ' + pongCount);
      pongCount++;
    });
  });

  return new Promise((resolve, reject) =>
  {
    httpServer.listen(port, () =>
    {
      console.log(`Listening on port ${port}`);
      resolve(wsServer);
    });
  });
}
