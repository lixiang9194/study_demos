import os
os.environ['TF_CPP_MIN_LOG_LEVEL']='2'
import tensorflow as tf
from tensorflow.examples.tutorials.mnist import input_data
import numpy as np

mnist = input_data.read_data_sets('data/mnist', one_hot=True)

with tf.name_scope('input_data'):
    # the input image is a row vector whoes size is 784
    input_images = tf.placeholder(tf.float32, shape=[128, 784], name='input_images')
    out_labels = tf.placeholder(tf.float32, shape=[None, 10], name='out_labels')
    # resize the image to 28x28x1, so we can do the convole operation
    x_image = tf.reshape(input_images,[-1,28,28,1])

global_step = tf.Variable(0, dtype=tf.int32, trainable=False, name='global_step')
# dropout control the rate to random throp data, which is to avoid over fit
drop_out = tf.placeholder(tf.float32, name='drop_out')


with tf.name_scope('conv1'):
    # input [128x28x28x1]
    kernel1 = tf.Variable(tf.truncated_normal([5,5,1,32]))
    bias1 = tf.Variable(tf.random_normal([32]), name='bias')
    conv10 = tf.nn.conv2d(x_image, kernel1, strides=[1,1,1,1], padding='SAME') 
    conv1 = tf.nn.relu(conv10 + bias1)

            # input [128x28x28x32]
with tf.name_scope('pool1'):
    pool1 = tf.nn.max_pool(conv1, ksize=[1,2,2,1], strides=[1,2,2,1], padding='SAME')

# input [128x14x14x32]
with tf.name_scope('conv2'):
    kernel2 = tf.Variable(tf.truncated_normal([5,5,32,64]), name='kernel')
    bias2 = tf.Variable(tf.random_normal([64]), name='bias')
    conv20 = tf.nn.conv2d(pool1, kernel2, strides=[1,1,1,1], padding='SAME') 
    conv2 = tf.nn.relu(conv20 + bias2)

# input [128x14x14x64]
with tf.name_scope('pool2'):
    pool2 = tf.nn.max_pool(conv2, ksize=[1,2,2,1], strides=[1,2,2,1], padding='SAME')

# input [128x7x7x64]
with tf.name_scope('fullcon'):
    pool32 = tf.reshape(pool2, [-1,7*7*64])
    weights3 = tf.Variable(tf.truncated_normal([7*7*64,1024]), name='weights')
    bias3 = tf.Variable(tf.random_normal([1024]), name='bias')
    fc_o = tf.nn.relu(tf.matmul(pool32, weights3) + bias3)
    fc = tf.nn.dropout(fc_o, drop_out, name='fc')

# input [128x1024]
with tf.name_scope('softmax'):
    weights4 = tf.Variable(tf.truncated_normal([1024,10]), name='weights')
    bias4 = tf.Variable(tf.random_normal(shape=[10]), name='bias')
    logists = tf.matmul(fc, weights4) + bias4
    # input [128x10] 
    out_conv = tf.nn.softmax(logists/1e5)

# define loss to optimizer the net
with tf.name_scope('loss'):
    loss = -tf.reduce_sum(out_labels*tf.log(out_conv))
    optimizer = tf.train.AdamOptimizer(0.01).minimize(loss)
    loss_average = tf.placeholder(tf.float32, name='loss_average')
    accuracy_average = tf.placeholder(tf.float32, name='accuracy_average')

# define accuracy to test the net
with tf.name_scope('accuracy'):
    correct_pred = tf.equal(tf.argmax(out_conv, 1), tf.argmax(out_labels, 1))
    accuracy = tf.reduce_mean(tf.cast(correct_pred, tf.float32))   

sess = tf.InteractiveSession()
sess.run(tf.global_variables_initializer())
# print sess.run(bias1)
# store the cnn net structure
writer = tf.summary.FileWriter('./data/mnist_cnn', sess.graph)

initial_step = 0
n_batches = int(mnist.train.num_examples/128.0)
n_test = int(mnist.test.num_examples/128.0)
total_loss = 0
for index in range(initial_step, initial_step + 100):

    X_batch, Y_batch = mnist.train.next_batch(128)

    l, lo, loss_batch = sess.run([logists,out_conv,accuracy], feed_dict={input_images:X_batch, out_labels:Y_batch, drop_out:0.5})
    #print sess.run(kernel1)
    print loss_batch

    
    optimizer.run(feed_dict={input_images:X_batch, out_labels:Y_batch, drop_out:0.5})
    # writer.add_summary(summary_batch, global_step=index)
    
   
   
