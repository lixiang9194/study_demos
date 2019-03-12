<?php
namespace think;

use Swoole\Http\Server;

class SwooleServer
{
    const HOST = "0.0.0.0";
    const PORT = 8866;

    private $server;

    public function __construct()
    {
        $this->server = new Server(self::HOST, self::PORT);
        
        $this->server->set([
            'enable_static_handler' => true,
            'document_root' => __DIR__.'/../public/static'
        ]);
    }

    //开启 http 服务
    public function serve()
    {
        //加载 tp 框架
        $this->server->on('WorkerStart', [$this, 'onWorkerStart']);

        $this->server->on('request', [$this, 'onRequest']);

        echo sprintf("start swoole http server on http://%s:%s\n", self::HOST, self::PORT);
        $this->server->start();
    }

    // 加载tp框架基础文件
    public function onWorkerStart($serv, $worker_id)
    {
        require __DIR__ . '/../thinkphp/base.php';
    }

    //处理请求
    public function onRequest($request, $response)
    {
        $this->initRequest($request);
        
        //捕获 tp 输出
        ob_start();
        
        Container::get('app')->run()->send();
        $res = ob_get_contents();
        ob_end_clean();

        //返回数据至客户端
        $response->end($res);

        $this->server->close($request->fd);
    } 

    private function initRequest($request)
    {
        // $_SERVER = [];
        // $_GET = [];
        // $_POST = [];

        if (isset($request->server)) {
            $this->setPara2Global($request->server, $_SERVER);
        } 
        if (isset($request->header)) {
            $this->setPara2Global($request->header, $_SERVER);
        }
        if (isset($request->get)) {
            $this->setPara2Global($request->get, $_GET);
        }
        if (isset($request->post)) {
            $this->setPara2Global($request->post, $_POST);
        }

    }

    private function setPara2Global($params, &$global)
    {
        foreach ($params as $key => $value) {
            $global[strtoupper($key)] = $value;
        }
    }
}

$server = new SwooleServer();
$server->serve();