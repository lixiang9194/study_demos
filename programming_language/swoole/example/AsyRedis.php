<?php

use Swoole\Redis;

class AsyRedis
{
    const HOST = '10.120.17.57';
    const PORT = 6379;

    private $client;
    
    function __construct()
    {
        $this->client = new Redis();
    }

    public function execute($action, $params)
    {
        //连接 redis
        $this->client->connect(self::HOST, self::PORT, function($client, $result) use ($action, $params) {
            if ($result === false) {
                echo "connect to redis server failed!\n";
                return;
            }

            //构造结果处理函数
            $params[] = function($client, $result) {
                    echo "result: {$result}\n";
                };

            //执行命令
            $client->__call($action, $params);

            //关闭连接
            $client->close();
        });
    }
}

$client = new AsyRedis();
//$client->execute('set', ['lzyKey', 'GoodGoodStudy']);
$client->execute('get', ['lzyKey']);