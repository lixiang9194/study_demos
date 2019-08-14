## Linux

* 输入输出
	0 标准输入    1标准输出    2标准错误    、dev/null文件
	> 输出重定向    < 输入重定向    >& 输出合并

* 查找命令
	find / -name 'example' 查找硬盘上的文件    grep -r -i 'example' * 查找文本内容

* 权限更改
	chown -R www:www path    chmod -R 777 path (rwx=>421)

* 文件统计
	wc [-wl] file

* xargs
	部分命令不接受标准输入，如kill、rm命令，xargs可将管道或标准输入换成命令行参数
	-d 指定分隔符    -p 询问执行    -n 指定一次传递几个参数	