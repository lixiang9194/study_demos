<?php

use Swoole\Async;


class AsyIO
{
    public function readFile($fileName)
    {
        Async::readFile($fileName, function($name, $content) {
            echo "fileName:{$name}".PHP_EOL;
            echo "content:\n{$content}".PHP_EOL;
        });
    }

    public function read($fileName)
    {
        Async::read($fileName, function($name, $content) use(&$i) {
            echo "content:\n{$content}".PHP_EOL;
            return false;
        }, 1024, 0);
    }

    public function writeFile($fileName, $content)
    {
        Async::writeFile($fileName, $content, function ($name) {
            echo "write {$name} ok!\n";
        }, FILE_APPEND);
    }
}


//测试
$readFile = __DIR__."/doupo.txt";
$writeFile = __DIR__."/FpmServer.php";

$io = new AsyIO();
$io->read($readFile);
//$io->writeFile($writeFile, "append content");

echo "program start!\n";