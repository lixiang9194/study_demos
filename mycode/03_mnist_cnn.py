import os
os.environ['TF_CPP_MIN_LOG_LEVEL']='2'
import tensorflow as tf
from tensorflow.examples.tutorials.mnist import input_data
import time
start = time.time()
mnist = input_data.read_data_sets("data/mnist", one_hot=True)

# define func to help initalize weights and bias
def weight_variable(shape):
    initial = tf.truncated_normal(shape)
    return tf.Variable(initial)

def bias_variable(shape):
    initial = tf.random_normal(shape)
    return tf.Variable(initial)

# define func to help convolve and pooling
def conv2d(x, W):
    # conv2d(input,filter,strides,padding,use_cudnn_on_gpu=true)
    # return : tensor(feature map)
    # input : input image,must be tensor, 
    return tf.nn.conv2d(x, W, strides=[1,1,1,1], padding='SAME')

def max_pool_2x2(x):
    return tf.nn.max_pool(x, ksize=[1,2,2,1], strides=[1,2,2,1], padding='SAME')

# define the placeholder
x = tf.placeholder(tf.float32, [None,784])
y_ = tf.placeholder(tf.float32, [None,10])

# define the 1st layer: a convolve and a pooling
W_conv1 = weight_variable([5,5,1,32])
b_conv1 = bias_variable([32])
x_image = tf.reshape(x, [-1,28,28,1])

# convolve the x_image with the weight, add the bias, apply the ReLU, finally max pool
h_conv1 = tf.nn.relu(conv2d(x_image, W_conv1) + b_conv1)
h_pool1 = max_pool_2x2(h_conv1)

# define the 2st layer
W_conv2 = weight_variable([5,5,32,64])
b_conv2 = bias_variable([64])
h_conv2 = tf.nn.relu(conv2d(h_pool1, W_conv2) + b_conv2)
h_pool2 = max_pool_2x2(h_conv2)

# define the ful connected layer
W_fc1 = weight_variable([7*7*64, 1024])
b_fc1 = bias_variable([1024])
h_pool2_flat = tf.reshape(h_pool2,[-1, 7*7*64])
h_fc1 = tf.nn.relu(tf.matmul(h_pool2_flat, W_fc1) + b_fc1)

# define dropout to aviod over fit
keep_prob = tf.placeholder(tf.float32)
h_fc1_drop = tf.nn.dropout(h_fc1, keep_prob)

# define the ouput
W_fc2 = weight_variable([1024, 10])
b_fc2 = bias_variable([10])
y_conv = tf.nn.softmax(tf.matmul(h_fc1_drop, W_fc2) + b_fc2)

# define the loss
cross_entropy = -tf.reduce_sum(y_*tf.log(y_conv))

# define the optimizer
optimizer = tf.train.AdamOptimizer(1e-4).minimize(cross_entropy)

# test the model
correct_prediction = tf.equal(tf.argmax(y_conv,1), tf.argmax(y_,1))
accuracy = tf.reduce_mean(tf.cast(correct_prediction, tf.float32))

# define the session
sess = tf.InteractiveSession()
sess.run(tf.global_variables_initializer())
out = sess.run(W_conv1)
print out
for i in range(20000):
    batch = mnist.train.next_batch(50)
    optimizer.run(feed_dict={x:batch[0], y_:batch[1], keep_prob:0.5})

    if i%1000 == 0:
        print '-----------------------------------'
        n_batches = int(mnist.test.num_examples/500)
        total_correct_preds = 0
        for i in range(n_batches):
            X_batch, Y_batch = mnist.test.next_batch(500)
            accuracy_batch = sess.run(accuracy, feed_dict={x:X_batch,y_:Y_batch, keep_prob:1.0})
            total_correct_preds += accuracy_batch
        print 'test accuracy {0}'.format(total_correct_preds/n_batches)

# calculate accuracy
print '-----------------------------------'
n_batches = int(mnist.test.num_examples/500)
total_correct_preds = 0
for i in range(n_batches):
    X_batch, Y_batch = mnist.test.next_batch(500)
    accuracy_batch = sess.run(accuracy, feed_dict={x:X_batch,y_:Y_batch, keep_prob:1.0})
    total_correct_preds += accuracy_batch
print 'test accuracy {0}'.format(total_correct_preds/n_batches)
print 'total time {0} seconds'.format(time.time() - start)