## go

* 数据类型
	bool
	byte(uint8的别名)
	整数 int int8/uint8 int16/uint16 int32/uint32 int64/uint64 uintptr
	浮点数 float32 float64 complex64 complex128
	string
	
	派生类型
		数组
		struct
		channel
		切片
		interface
		Map

* go scheduler
	将goroutines按照一定算法放到不同的操作系统线程种去执行，在语言层面自带调度器，因此称为原生支持并发。
	GPM模型
		G: goroutine,存储了goroutine的执行stack、状态、任务函数等信息
		P: 逻辑processor，P的数量决定了系统内最大可并行的G的数量，P拥有各种G对象队列、链表、cache和状态
		M: M代表真正的计算资源。绑定有效P后，进入schedule循环，从队列获取G,切换到G的栈上执行，完毕后返回M。M并不保留G状态，所以可以跨M调度。

	抢占式调度
		Go 1.2中实现抢占式调度，在每个函数或方法入口，让runtime检查是否需要执行抢占式调度，但仍然只是部分解决了饿死问题，对于没有函数计算，纯算法循环的G，scheduler仍然无法抢占。

* channel
	make(chan int) 无缓冲channel,双方同步，否则阻塞
	make(chan int, n) 带缓冲区的channel，满则阻塞
