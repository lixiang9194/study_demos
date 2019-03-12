<?php

use Swoole\WebSocket\Server;

class WsServer
{
    const HOST = "0.0.0.0";
    const PORT = 8877;

    private $server;
    
    function __construct()
    {
        $this->server = new Swoole\WebSocket\Server(self::HOST, self::PORT);
    }

    public function serve()
    {
        //WebSocket 服务
        $this->server->on('open', [$this, 'onOpen']);
        $this->server->on('message', [$this, 'onMessage']);
        $this->server->on('close', [$this, 'onClose']);

        //http服务
        $this->server->on('request', [$this, 'onRequest']);

        //启动服务器
        echo sprintf("start server at %s:%s\n", self::HOST, self::PORT);
        $this->server->start();
        
    }

    public function onOpen($server, $request)
    {
        echo "server: handshake success with fd{$request->fd}\n";
        swoole_timer_tick(1000, function ($timer_id, $fd) {
            $this->server->push($fd, "hello {$fd}, server is still alive!");
        }, $request->fd);
    }

    public function onMessage($server, $frame)
    {
        echo "receive from {$frame->fd}:{$frame->data}, opcode:{$frame->opcode}, fin:{$frame->finish}\n";
        $server->push($frame->fd, "this is server");
    }

    public function onClose($server, $fd)
    {
        echo "client {$fd} closed\n";
    }

    public function onRequest($request, $response)
    {
        $response->end("hello {$request->fd}\n");
    }
}

$ws = new WsServer();
$ws->serve();