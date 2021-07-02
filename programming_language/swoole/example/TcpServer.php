<?php

class Server
{
    private $serv;

    public function __construct()
    {
        $this->serv = new swoole_server("127.0.0.1", 1992);
        $this->serv->set(array(
            'worker_num' => 8,
            'daemonize' => false,
            'heartbeat_check_interval' => 30
        ));

        $this->serv->on('Start', array($this, 'onStart'));
        $this->serv->on('Connect', array($this, 'onConnect'));
        $this->serv->on('Receive', array($this, 'onReceive'));
        $this->serv->on('Close', array($this, 'onClose'));

        $this->serv->start();
    }

    public function onStart($serv)
    {
        echo "Server Start\n";
    }

    public function onConnect($serv, $fd, $from_id)
    {
        $serv->send($fd, "Hello {$fd}!");
    }

    public function onReceive(swoole_server $serv, $fd, $from_id, $data)
    {
        echo "get message from client {$fd}:{$data}\n";
        $serv->send($fd, $data);
    }

    public function onClose($serv, $fd, $from_id)
    {
        echo "client {$fd} close connection\n";
    }
}

$server = new Server();