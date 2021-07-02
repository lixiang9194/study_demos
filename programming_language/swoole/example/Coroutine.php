<?php

use Swoole\Http\Server;
use Swoole\Coroutine\Redis;

class Coroutine
{
    private $http;
    
    function __construct()
    {
        $this->http = new Server('0.0.0.0', 8866);

        $this->http->on('request', function($request, $response) {
            
            //connect redis
            $redis = new Redis();
            $redis->connect('10.120.17.57', 6379);
            
            //query data
            $value = $redis->get($request->get['key']);
            
            //send data
            $response->end($value);
        });

        //start server
        $this->http->start();
    }
}

$c = new Coroutine();