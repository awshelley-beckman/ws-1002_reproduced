import * as WebSocket from 'websocket';
import * as http from 'http';
import * as readline from 'readline';

const wsConnections = new Set<WebSocket.connection>();

async function main()
{
  const server = await startServer();

  const messageLen = 0;
  const pingData = Buffer.alloc(messageLen);
  pingData.fill('a', 0, messageLen);

  console.log('built the big ping message');
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

function pongAll(data: Buffer)
{
  wsConnections.forEach(conn => {
    conn.pong(data);
  });
}

function readLineAsync(): Promise<string>
{
  const readLineInterface = readline.createInterface(
    {
      input: process.stdin,
      output: process.stdout
    }
  );

  return new Promise<string>((resolve, reject) =>
  {
    readLineInterface.question('Enter ping data', (answer) =>
    {
      readLineInterface.close();
      resolve(answer);
    });
  })
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

    // Respond to all messages with "hello".
    connection.on('message', req =>
    {
      console.log(`Server received ${req.utf8Data}`);
      connection.sendUTF('Hello');
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

function startClient(url: string) : Promise<WebSocket.connection>
{
  return new Promise((resolve, reject) =>
  {
    console.log(`Connecting to ${url}`);

    const client = new WebSocket.client();
    client.on('connectFailed', err => console.log('Test client connection error: ' + err));
    client.on('connect', connection =>
    {
      console.log('Client received connection');
      connection.on('message', msg =>
      {
        console.log(`Client received ${msg.utf8Data}`);
      });
      resolve(connection);
    });

    console.log('about to connect client');
    client.connect(url, 'echo-protocol');
  })
}
