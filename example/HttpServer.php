<?php

$http = new swoole_http_server("127.0.0.1", 8086);
$http->on('request', function ($request, $response) {
    $response->end("hello from swoole server!\n");
});

$http->start();