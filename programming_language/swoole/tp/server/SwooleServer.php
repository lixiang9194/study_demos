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
        $this->initGlobal($request);
        
        //捕获 tp 输出
        ob_start();

        Container::get('app')->run()->send();
        
        $res = ob_get_contents();
        ob_end_clean();

        //返回数据至客户端
        $response->end($res);
        
        //清理php超全局变量
        $this->clearGlobal();
        
        //$this->server->close();
    } 

    private function initGlobal($request)
    {
        if (isset($request->server)) {
            foreach ($request->server as $key => $value) {
                $_SERVER[strtoupper($key)] = $value;
            }
        }
        if (isset($request->header)) {
            foreach ($request->header as $key => $value) {
                $_SERVER[strtoupper($key)] = $value;
            }
        }

        //由于Request实例化后pathinfo()函数实现
        //使用swoole服务器需设置此参数才能获取正确的请求路径
        $_SERVER['argv'][1] = $_SERVER['REQUEST_URI'];

        if (isset($request->get)) {
            foreach ($request->get as $key => $value) {
                $_GET[$key] = $value;
            }
        }
        if (isset($request->post)) {
            foreach ($request->post as $key => $value) {
                $_POST[$key] = $value;
            }
        }
    }

    private function clearGlobal()
    {
        $_SERVER = [];
        $_GET = [];
        $_POST = [];
    }
}

$server = new SwooleServer();
$server->serve();