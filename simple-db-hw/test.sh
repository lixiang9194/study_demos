
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

