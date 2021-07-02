import os
os.environ['TF_CPP_MIN_LOG_LEVEL']='2'
import tensorflow as tf


def sess_intro():
    x = 2
    y = 3
    add_op = tf.add(x, y)
    mul_op = tf.multiply(x, y)
    useless = tf.multiply(x, add_op)
    pow_op = tf.pow(add_op, mul_op)
    with tf.Session() as sess:
        z, not_useless = sess.run([pow_op, useless])
        print z,not_useless


def graph_intro():
    g1 = tf.Graph()
    g2 = tf.get_default_graph()      # get default graph

    with g1.as_default():
        x = tf.add(3,5,name='asdf')
        p = tf.multiply(3,x,name='zxcv')

    with g2.as_default():
        a = tf.constant(3)      # add constant

    sess1 = tf.Session(graph=g1)
    sess2 = tf.Session(graph=g2)
    print sess1.graph.as_graph_def()
    writer = tf.summary.FileWriter('./graphs', sess1.graph)
    print sess1.run(p),sess2.run(a)
    sess1.close()
    sess2.close()
# tensorboard --logdir='./graphs' --port 6006

def op_intro():
    a = tf.constant([2,2],name='a')
    b =  tf.constant([[0,1],[2,3]],name='b')
    x = tf.add(a,b,name='add')
    y = tf.multiply(a,b,name='multiply')
    with tf.Session() as sess:
        writer = tf.summary.FileWriter('./graphs', sess.graph)
        print sess.graph.as_graph_def()
        x,y = sess.run([x,y])
        print x
        print '-------------'
        print y


def vari_intro():
    a = tf.Variable(2, name='scalar')
    b = tf.Variable([2,3],name='vector')
    c = tf.Variable([[0,1],[2,3]],name='matrix')
    pl = tf.placeholder(tf.int32,shape=[2])
    add = tf.add(a,b,name='add')
    mul = tf.multiply(b,c,name='multiply')
    ma = tf.add(b,pl,name='ma')
    with tf.Session() as sess:
        writer = tf.summary.FileWriter('./graphs/12', sess.graph)
        print sess.graph.as_graph_def()
        sess.run(b.initializer)
        print sess.run(ma,feed_dict={pl:[5,6]})
        print sess.run(ma,{pl:[2,4]})


if __name__ == '__main__':
    vari_intro()