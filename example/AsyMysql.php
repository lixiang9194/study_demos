<?php

use Swoole\Mysql;


class AsyMysql
{
    private $client;
    private $dbConfig;

    function __construct()
    {
        $this->client = new Mysql();

        //database config
        $this->dbConfig = [
            'host' => 'cube.db.master',
            'port' => 3306,
            'user' => 'Cube_usr_tst',
            'password' => 'PAT&ppu3792',
            'database' => 'cube',
            'charset' => 'utf8mb4',
            'timeout' => 1,
        ];

        
    }

    public function query($sql)
    {
        //connect to database
        $this->db->connect($this->dbConfig, function ($db, $res) use ($sql){
            if ($res === false) {
                var_dump($db->connect_errno, $db->connect_error);
                die;
            }

            //execute query
            $this->db->query($sql, function ($db, $res) {
                if ($res === false) {
                    var_dump($db->error, $db->errno);
                } else if ($res === true) {
                    var_dump($db->affected_rows, $db->insert_id);
                } else {
                    print_r($res);
                }

                //close conn
                $db->close();
            }); 
        });
        
    }
}

$am = new AsyMysql();
$am->query("select * from ccmc_engineer_transfer limit 1;");