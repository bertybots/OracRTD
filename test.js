'use strict';

var port = 8200;

var curvepoints = {};

var dirty = true;
var dataCount = 0;
var calcCount = 0;

const SocketIoServer = require('socket.io');

const ioClients = new SocketIoServer(port);
console.log('Client Proxy Server started on port: ' + port);

ioClients.on('connection', function (client) {
    console.log('Client Connected, id: ' + client.id);

    client.on('disconnect', function (data) {
        console.log('Client Disconnected, id: ' + data.id);
    });

    client.emit('curve', curvepoints);

    client.on('publish', function (data) {
        try {
            var nvp = JSON.parse(data);
            if (nvp.name != null && nvp.value != null) {
                curvepoints[nvp.name] = nvp.value;
                dirty = true;
                dataCount++;
            }
        } catch (error) {
            console.error('Error proccessing publish: ' + error);
        }
    });
});

setInterval(() => {
    console.error('Sockets: ' + Object.keys(ioClients.sockets.connected).length);
    console.error('Data: ' + dataCount);
    dataCount = 0;
    if (dirty) {
        console.error('Publishing Curve...');
        ioClients.emit('curve', curvepoints);
        console.error('Published Curve: ' + calcCount++);
        dirty = false;
    }
}, 500);
