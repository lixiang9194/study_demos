
#lab1
ant runtest -Dtest=TupleTest
ant runtest -Dtest=TupleDescTest
ant runtest -Dtest=CatalogTest
ant runtest -Dtest=HeapPageIdTest
ant runtest -Dtest=RecordIDTest
ant runtest -Dtest=HeapPageReadTest
ant runtest -Dtest=HeapFileReadTest
ant runsystest -Dtest=ScanTest

#try
java -jar dist/simpledb.jar convert test_seq_file.txt 3
java -jar dist/simpledb.jar print test_seq_file.dat 3
ant
java -classpath dist/simpledb.jar simpledb.TestSeq


#lab2
ant runtest -Dtest=PredicateTest
ant runtest -Dtest=FilterTest
ant runtest -Dtest=JoinPredicateTest
ant runtest -Dtest=JoinTest
ant runsystest -Dtest=FilterTest
ant runsystest -Dtest=JoinTest
ant runtest -Dtest=StringAggregatorTest
ant runtest -Dtest=IntegerAggregatorTest
ant runtest -Dtest=AggregateTest
ant runsystest -Dtest=AggregateTest
ant runtest -Dtest=HeapPageWriteTest
ant runtest -Dtest=HeapFileWriteTest
ant runtest -Dtest=BufferPoolWriteTest
ant runsystest -Dtest=DeleteTest
ant runtest -Dtest=InsertTest
ant runsystest -Dtest=InsertTest

#try
java -jar dist/simpledb.jar convert test_join_file1.txt 3
java -jar dist/simpledb.jar convert test_join_file2.txt 3
ant
java -classpath dist/simpledb.jar simpledb.TestJoin