<?php

//快排
//时间复杂度 O(nlogn) 空间复杂度 O(1) 不稳定
class QuickSort
{
	public static function sort(&$array)
	{
		self::qSort($array, 0, count($array)-1);
	}

	protected static function qSort(&$array, $left, $right)
	{
		if($left >= $right) {
			return;
		}

		$mid = self::partion($array, $left, $right);

		self::qSort($array, $left, $mid-1);
		self::qSort($array, $mid+1, $right);
	}

	protected static function partion(&$array, $left, $right)
	{
		$index = $array[$left];
		while($left < $right) {
			while ($right > $left && $array[$right] >= $index) {$right--;}
			$array[$left] = $array[$right];
			while($left < $right && $array[$left] <= $index) {$left++;}
			$array[$right] = $array[$left];
		}
		$array[$left] = $index;
		return $left;
	}
}

//冒泡排序
//时间复杂度 O(n2) 空间复杂度 O(1) 稳定
class BubbleSort
{
	public static function sort(&$array)
	{
		$cnt = count($array);
		$end = $last = $cnt-1;
		for($i=$cnt; $i>$end-1; $i--) {
			$end = $last;$last = 0;
			for($j=0; $j<$end; $j++) {
				if ($array[$j] > $array[$j+1]) {
					$temp = $array[$j];
					$array[$j] = $array[$j+1];
					$array[$j+1] = $temp;
					$last = $j; 
				}
			}
		}
	}
}

//插入排序
//时间复杂度 O(n2) 空间复杂度 O(1) 稳定
class InsertSort
{
	public static function sort(&$array)
	{
		$cnt = count($array);
		for ($i=0; $i < $cnt; $i++) {
			for ($j=$i; $j > 0; $j--) {
				if($array[$j] < $array[$j-1]) {
					$temp = $array[$j];
					$array[$j] = $array[$j-1];
					$array[$j-1] = $temp;
				}
			}
		}
	}
}

//选择排序
//时间复杂度 O(n2) 空间复杂度 O(1) 不稳定
class SelectSort
{
	public static function sort(&$array)
	{
		$cnt = count($array);
		for ($i=0; $i < $cnt; $i++) {
			$min = $i;
			for ($j=$i+1; $j<$cnt; $j++) {
				if ($array[$j] < $array[$min]) {
					$min = $j;
				}
			}
			if ($min != $i) {
				$temp = $array[$i];
				$array[$i] = $array[$min];
				$array[$min] = $temp;
			}
		}
	}
}

//归并排序
//时间复杂度 O(nlogn) 空间复杂度 O(n) 稳定
class MergeSort
{
	public static function sort(&$array)
	{
		self::mSort($array, 0, count($array)-1);
	}

	public static function mSort(&$array, $left, $right)
	{
		if ($left >= $right) {
			return;
		}

		$mid = floor(($left+$right)/2);
		self::mSort($array, $left, $mid);
		self::mSort($array, $mid+1, $right);
		self::merge($array, $left, $mid, $right);
	}

	public static function merge(&$array, $left, $mid, $right)
	{
		$i=$left;$j=$mid+1;
		$la = $array;$index=$left;
		while($i<=$mid && $j<=$right) {
			if($la[$i] > $array[$j]) {
				$array[$index++] = $array[$j++];
			} else {
				$array[$index++] = $la[$i++];
			}
		}

		while($i<=$mid) {
			$array[$index++] = $la[$i++];
		}
		while($j<=$right) {
			$array[$index++] = $array[$j++];
		}
	}
}

//堆排序
//时间复杂度 O(nlogn) 空间复杂度 O(1) 稳定
class HeapSort
{
	public static function sort(&$array)
	{
		$length = count($array);
		$mid = floor($length/2) -1 ;
		for($i=$mid; $i>=0; $i--) {
			self::adjustHeap($array, $i, $length);
		}
		for($i=$length-1; $i>0; $i--) {
			$temp = $array[0];
			$array[0] = $array[$i];
			$array[$i] = $temp;
			self::adjustHeap($array, 0, $i);
		}
	}

	public static function adjustHeap(&$array, $index, $length)
	{
		$temp = $array[$index];
		for ($i=2*$index+1; $i<$length; $i=2*$i+1) {
			if ($i<$length-1 && $array[$i]<$array[$i+1]) {
				$i++;
			}
			if ($array[$i] > $temp) {
				$array[$index] = $array[$i];
				$index = $i;
			} else {
				break;
			}
		}
		$array[$index] = $temp;
	}
}


$arr = [4,2,5,6,6,3,5,9,4,9,6,8,1,2,12,1,3,33,9,0,0,3,8,8,6,6,4,4,4,33,22,21,34,67,45,45,34,78,34,44];
HeapSort::sort($arr);
print_r($arr);

