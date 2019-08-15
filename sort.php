<?php

//快排
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
class BubbleSort
{
	public static function sort(&$array)
	{
		$count = count($array);
		$end = $last = $count-1;
		for($i=0; $i<$count && $end>0; $i++) {
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

$arr = [1,4,2,5,6,6,3,3,5,9,4,9,6,8,1,2,12,1,3,33,9,0,0,3,8,8,6,6,4,4,4,33,22,21,34,67,45,45,34,78,34,44];
BubbleSort::sort($arr);
print_r($arr);