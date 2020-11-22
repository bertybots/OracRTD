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
        console.log('Client Disconnected, id: ' + client.id);
    });

    client.on('listen', function (data) {
        console.log('Client id: ' + client.id + ' - Listening to ' + data);
        client.join(data);
        if (data == 'curveInputs') {
            client.emit('curveInputs', curvepoints);
        }
    });

    client.on('cooked', function (data) {
        console.log('Cooked');
        ioClients.in('cooked').emit('cooked', data);
    });

    client.on('publish', function (data) {
        try {
            var nvp = JSON.parse(data);
            if (nvp.name != null && nvp.value != null) {
                var dt = new Date();
                var md = {
                    name: nvp.name,
                    value: nvp.value,
                    username: nvp.username,
                    datetime: dt,
                    excelDate: JSDateToExcelDate(dt)
                }
                curvepoints[nvp.name] = md;
                dirty = true;
                dataCount++;
            }
        } catch (error) {
            console.error('Error proccessing publish: ' + error);
        }
    });
});

function JSDateToExcelDate(inDate) {
    var returnDateTime = 25569.0 + ((inDate.getTime() - (inDate.getTimezoneOffset() * 60 * 1000)) / (1000 * 60 * 60 * 24));
    return returnDateTime.toString().substr(0,20); 
}

setInterval(() => {
    console.error('Sockets: ' + Object.keys(ioClients.sockets.connected).length);
    console.error('Data: ' + dataCount);
    dataCount = 0;
    if (dirty) {
        console.error('Publishing Curve...');
        ioClients.in('curveInputs').emit('curveInputs', curvepoints);
        console.error('Published Curve: ' + calcCount++);
        dirty = false;
    }
}, 50);
