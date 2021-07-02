import os
os.environ['TF_CPP_MIN_LOG_LEVEL']='2'

import tensorflow as tf
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

DATA_FILE = '../stanford-tensorflow-tutorials/data/fire_theft.xls'

# define huber loss function 
def huber_loss(labels,predictions,delta=1.0):
    residual = tf.abs(predictions-labels)
    condition = tf.less(residual, delta)
    small_res = 0.5*tf.square(residual)
    large_res = delta*residual-0.5*tf.square(delta)
    return tf.where(condition,small_res,large_res)

# step 1: read in data
data = pd.read_excel(DATA_FILE,0,header=0)

X_data = data.iloc[:,0]
Y_data = data.iloc[:,1]

# plot the original data to see the trending
# plt.scatter(X_data,y_data)
# plt.show()

# setp 2: create placeholders for X and Y
X = tf.placeholder(tf.float32, name='X')
Y = tf.placeholder(tf.float32, name='Y')


# step 3: create weight and bias, initialized to 0

w3 = tf.Variable(0.0, name='w3')
w2 = tf.Variable(0.0, name='w2')
w = tf.Variable(0.0, name='weights')
b = tf.Variable(0.0, name='bias')

# step 4: build model to predict Y
Y_predicted =  X * X * X * w3 + X * X * w2 + X * w + b

# step 5: use the square error as the loss func
loss = tf.reduce_mean(tf.square(Y - Y_predicted, name='loss'))
# loss = tf.reduce_sum(huber_loss(Y, Y_predicted))

# step 6: using gradient descent with learning rate of 0.01
optimizer = tf.train.AdamOptimizer(learning_rate=1e-2).minimize(loss)

with tf.Session() as sess:
    #step 7: initalize the necessary variables
    sess.run(tf.global_variables_initializer())

    writer = tf.summary.FileWriter('./graphs/linear_reg',sess.graph)

    # step 8:train the model
    size = X_data.size
    train_loss = []
    for i in range(50):
        _, l = sess.run([optimizer,loss],feed_dict={X:X_data,Y:Y_data})
        train_loss.append(l)
        print 'Epoch {0}: {1}'.format(i, l)

    writer.close()

    # step 9: output the values of w and b
    w, b = sess.run([w, b])

    # plot the results
    plt.scatter(X_data, Y_data, label='Real data')
    plt.plot(X_data,Y_predicted.eval(feed_dict={X:X_data}), 'ro', label='Predicted data')
    # plt.plot(train_loss, 'r', label='train loss')
    plt.legend()
    plt.show()


