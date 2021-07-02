import os
os.environ['TF_CPP_MIN_LOG_LEVEL']='2'
import tensorflow as tf
from tensorflow.examples.tutorials.mnist import input_data


class Mnist_Cnn:
    # """a simple cnn network to train mnist """
    def __init__(self):
        pass
        # tf.set_random_seed(13)

    def _create_para(self):
        with tf.device('/cpu:0'):
            with tf.name_scope('input_data'):
                # the input image is a row vector whoes size is 784
                self.input_images = tf.placeholder(tf.float32, shape=[128, 784], name='input_images')
                self.out_labels = tf.placeholder(tf.float32, shape=[128, 10], name='out_labels')
                # resize the image to 28x28x1, so we can do the convole operation
                self.x_image = tf.reshape(self.input_images,[-1,28,28,1])

        with tf.device('/cpu:0'):
            self.global_step = tf.Variable(0, dtype=tf.int32, trainable=False, name='global_step')
            # dropout control the rate to random throp data, which is to avoid over fit
            self.drop_out = tf.placeholder(tf.float32, name='drop_out')

    def _create_conv(self):
        with tf.device('/cpu:0'):
            with tf.name_scope('conv1'):
                # input [128x28x28x1]
                self.kernel1 = tf.Variable(tf.truncated_normal([5,5,1,32]), name='kernel')
                bias = tf.Variable(tf.random_normal([32]), name='bias')
                conv = tf.nn.conv2d(self.x_image, self.kernel1, strides=[1,1,1,1], padding='SAME') 
                self.conv1 = tf.nn.relu(conv + bias)

            # input [128x28x28x32]
        with tf.device('/cpu:0'):
            with tf.name_scope('pool1'):
                self.pool1 = tf.nn.max_pool(self.conv1, ksize=[1,2,2,1], strides=[1,2,2,1], padding='SAME')

            # input [128x14x14x32]
        with tf.device('/cpu:0'):
            with tf.name_scope('conv2'):
                kernel = tf.Variable(tf.truncated_normal([5,5,32,64]), name='kernel')
                bias = tf.Variable(tf.random_normal([64]), name='bias')
                conv = tf.nn.conv2d(self.pool1, kernel, strides=[1,1,1,1], padding='SAME') 
                self.conv2 = tf.nn.relu(conv + bias)

            # input [128x14x14x64]
        with tf.device('/cpu:0'):
            with tf.name_scope('pool2'):
                self.pool2 = tf.nn.max_pool(self.conv2, ksize=[1,2,2,1], strides=[1,2,2,1], padding='SAME')

            # input [128x7x7x64]
        with tf.device('/cpu:0'):
            with tf.name_scope('fullcon'):
                pool2 = tf.reshape(self.pool2, [-1,7*7*64])
                weights = tf.Variable(tf.truncated_normal([7*7*64,1024]), name='weights')
                bias = tf.Variable(tf.constant(0.0, shape=[1024]), name='bias')
                fc_o = tf.nn.relu(tf.matmul(pool2, weights) + bias)
                self.fc = tf.nn.dropout(fc_o, self.drop_out, name='fc')

            # input [128x1024]
        with tf.device('/cpu:0'):
            with tf.name_scope('softmax'):
                weights = tf.Variable(tf.truncated_normal([1024,10]), name='weights')
                bias = tf.Variable(tf.constant(0.0, shape=[10]), name='bias')
                logists = tf.matmul(self.fc, weights) + bias
                # input [128x10] 
                self.out_conv = tf.nn.softmax(logists/1e5)

            # define loss to optimizer the net
        with tf.device('/cpu:0'):
            with tf.name_scope('loss'):
                self.loss = -tf.reduce_sum(self.out_labels*tf.log(self.out_conv))
                self.optimizer = tf.train.AdamOptimizer(1e-4).minimize(self.loss, global_step=self.global_step)
                self.loss_average = tf.placeholder(tf.float32, name='loss_average')
                self.accuracy_average = tf.placeholder(tf.float32, name='accuracy_average')
            
            # define accuracy to test the net
        with tf.device('/cpu:0'):
            with tf.name_scope('accuracy'):
                correct_pred = tf.equal(tf.argmax(self.out_conv, 1), tf.argmax(self.out_labels, 1))
                self.accuracy = tf.reduce_mean(tf.cast(correct_pred, tf.float32))
            
            # define summaries to show on tensorboard
        with tf.device('/cpu:0'):
            with tf.name_scope('summary_batch'):
                lbs = tf.summary.scalar('loss_batch', self.loss)
                lbh = tf.summary.histogram('loss_batch', self.loss)
                # wbs = tf.summary.scalar('weight1', tf.reshape(self.kernel1,[-1]))
                wbh = tf.summary.histogram('weight1', self.kernel1)
                self.summary_batch = tf.summary.merge([lbs,lbh,wbh])

        with tf.device('/cpu:0'):
            with tf.name_scope('summary_average'):
                lat = tf.summary.scalar('loss_average', self.loss_average)
                lah = tf.summary.histogram('loss_average', self.loss_average)
                aas = tf.summary.scalar('accuracy', self.accuracy_average)
                aah = tf.summary.histogram('accuracy', self.accuracy_average)
                self.summary_average = tf.summary.merge([lat, lah, aas, aah])
    
                #self.summary_average = tf.summary.merge_all()
    
    def build_cnn(self):
        self._create_para()
        self._create_conv()

    def train_cnn(self, mnist, n_epocs=10, drop_out=0.5):
        sess = tf.InteractiveSession()
        sess.run(tf.global_variables_initializer())
        
        # store the cnn net structure
        writer = tf.summary.FileWriter('./data/mnist_cnn01', sess.graph)
        # store and check the progress
        saver = tf.train.Saver()
        ckpt = tf.train.get_checkpoint_state('./data/mnist_cnn01')
        if ckpt and ckpt.model_checkpoint_path:
            saver.restore(sess, ckpt.model_checkpoint_path)

        initial_step = self.global_step.eval()
        n_batches = int(mnist.train.num_examples/128.0)
        n_test = int(mnist.test.num_examples/128.0)
        total_loss = 0
        for index in range(initial_step, initial_step + n_batches*n_epocs):
            X_batch, Y_batch = mnist.train.next_batch(128)
            _, loss_batch, summary_batch = sess.run([self.optimizer, self.loss, self.summary_batch], 
                                feed_dict={self.input_images:X_batch, self.out_labels:Y_batch, self.drop_out:drop_out})
            writer.add_summary(summary_batch, global_step=index)
            total_loss += loss_batch
            # display error and accuracy every 100 iter
            if (index + 1) % 100 == 0:
                average_loss = total_loss/100.0
                print ('Average loss at step {}: {:5.1f}'.format(index+1, average_loss))
                total_loss = 0.0
                saver.save(sess, './data/mnist_cnn01/mnist-cnn', index)
            
            accuracy_average = 0.0
            if (index + 1) % 1000 == 0:    
                for indexj in range(n_test):
                    x_test, y_test = mnist.test.next_batch(128)
                    accuracy_batch = sess.run(self.accuracy, 
                        feed_dict={self.input_images:x_test, self.out_labels:y_test,self.drop_out:1.0})
                    accuracy_average += accuracy_batch
                accuracy_average /= n_test
                print ('Average accuracy at step {}: {:5.5f}'.format(index+1, accuracy_average))
                summary_average = sess.run(self.summary_average,
                        feed_dict={self.loss_average:average_loss, self.accuracy_average:accuracy_average})
                writer.add_summary(summary_average, global_step=index)

        print('Optimization Finished!')
        accuracy_average = 0.0
        for indexj in range(n_test):
            x_test, y_test = mnist.test.next_batch(128)
            accuracy_batch = sess.run(self.accuracy, 
                feed_dict={self.input_images:x_test, self.out_labels:y_test, self.drop_out:1.0})
            accuracy_average += accuracy_batch
        accuracy_average /= n_test
        print("Accuracy {0}".format(accuracy_average))


def main():
    mnist = input_data.read_data_sets('./data/mnist', one_hot=True)
    cnn = Mnist_Cnn()
    cnn.build_cnn()
    cnn.train_cnn(mnist)

if __name__ == '__main__':
    main()