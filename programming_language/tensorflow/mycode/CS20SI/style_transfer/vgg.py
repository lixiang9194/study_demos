import sys
import tensorflow as tf
import numpy as np
from utils import download_vgg, fit_img
import scipy.io


class VGG_NET:
    # load the vgg mat with scipy.io
    def __init__(self, vgg_weights_path, synset_path):
        vgg = scipy.io.loadmat(vgg_weights_path)
        self.vgg_layers = vgg['layers'][0]
        self.synset = self.__load_synset(synset_path)
        self.graph = {}


    # load the correding text for the prob
    def __load_synset(self, synset_path):
        synset = {}
        with open(synset_path, 'r') as f:
            index = 0
            for line in f.readlines():
                synset[index] = line[10:]
                index += 1
        return synset


    # parse vgg_layers to get w,b for expected layer
    def __get_weights(self, layer, expected_layer_name):
        w = self.vgg_layers[layer][0][0][2][0][0]
        b = self.vgg_layers[layer][0][0][2][0][1]
        layer_name = self.vgg_layers[layer][0][0][0][0]
        assert layer_name == expected_layer_name
        return w,b.reshape(b.size)


    # construct the conv2d layer using the weights and bias
    def __construct_conv2d_relu(self, prev_layer, layer, layer_name):
        # specify variable_scope to avoid varibale name conflict
        with tf.variable_scope(layer_name) as scope:
            w, b = self.__get_weights(layer, layer_name)
            # w, b are numpy arrays, convert them to tf tensors
            w = tf.constant(w, name='weights')
            b = tf.constant(b, name='bias')
            # apply convolution on the pre_layer with kernel w
            # then apply relu with bias b
            conv2d = tf.nn.conv2d(prev_layer, w, strides=[1,1,1,1], padding='SAME')
            relu = tf.nn.relu(conv2d + b)
            return relu
    

    # construct the average pooling layer
    def __construct_avg_pool(self, pre_layer, layer_name):
        # the paper suggest that average pooling actually works better than max pooling
        avg_pool = tf.nn.avg_pool(pre_layer, ksize=[1,2,2,1], strides=[1,2,2,1],
                                padding='SAME', name=layer_name)
        return avg_pool


    # construct the full connect layer
    def __construct_fc_relu(self, pre_layer,layer, layer_name, softmax=False):
        with tf.variable_scope(layer_name) as scope:
            w, b = self.__get_weights(layer, layer_name)
            # reshape the input(pre_layer) and weights(w) 
            shape = w.shape
            dim = 1
            for d in shape[:-1]:
                dim *= d
            w = np.reshape(w, [dim, -1])
            pre_layer = tf.reshape(pre_layer, [-1, dim])
            
            w = tf.constant(w, name='weights')
            b = tf.constant(b, name='bias')

            fc = tf.matmul(pre_layer, w) + b
            # in the last fc layer, there is no relu 
            if softmax:
                return fc
            return tf.nn.relu(fc)


    # construct the vgg_net
    def construct_vgg_net(self, batch_size=1, image_height=224, image_width=224):
        self.input_image = tf.Variable(np.zeros([batch_size, image_height, image_width, 3]), dtype=tf.float32)
        graph = {}
        graph['conv1_1'] = self.__construct_conv2d_relu(self.input_image, 0, 'conv1_1')
        graph['conv1_2'] = self.__construct_conv2d_relu(graph['conv1_1'], 2, 'conv1_2')
        graph['avg_pool1'] = self.__construct_avg_pool(graph['conv1_2'], 'avg_pool1')
        graph['conv2_1'] = self.__construct_conv2d_relu(graph['avg_pool1'], 5, 'conv2_1')
        graph['conv2_2'] = self.__construct_conv2d_relu(graph['conv2_1'], 7, 'conv2_2')
        graph['avg_pool2'] = self.__construct_avg_pool(graph['conv2_2'], 'avg_pool2')
        graph['conv3_1'] = self.__construct_conv2d_relu(graph['avg_pool2'], 10, 'conv3_1')
        graph['conv3_2'] = self.__construct_conv2d_relu(graph['conv3_1'], 12, 'conv3_2')
        graph['conv3_3'] = self.__construct_conv2d_relu(graph['conv3_2'], 14, 'conv3_3')
        graph['conv3_4'] = self.__construct_conv2d_relu(graph['conv3_3'], 16, 'conv3_4')
        graph['avg_pool3'] = self.__construct_avg_pool(graph['conv3_4'], 'avg_pool3')
        graph['conv4_1'] = self.__construct_conv2d_relu(graph['avg_pool3'], 19, 'conv4_1')
        graph['conv4_2'] = self.__construct_conv2d_relu(graph['conv4_1'], 21, 'conv4_2')
        graph['conv4_3'] = self.__construct_conv2d_relu(graph['conv4_2'], 23, 'conv4_3')
        graph['conv4_4'] = self.__construct_conv2d_relu(graph['conv4_3'], 25, 'conv4_4')
        graph['avg_pool4'] = self.__construct_avg_pool(graph['conv4_4'], 'avg_pool4')
        graph['conv5_1'] = self.__construct_conv2d_relu(graph['avg_pool4'], 28, 'conv5_1')
        graph['conv5_2'] = self.__construct_conv2d_relu(graph['conv5_1'], 30, 'conv5_2')
        graph['conv5_3'] = self.__construct_conv2d_relu(graph['conv5_2'], 32, 'conv5_3')
        graph['conv5_4'] = self.__construct_conv2d_relu(graph['conv5_3'], 34, 'conv5_4')
        graph['avg_pool5'] = self.__construct_avg_pool(graph['conv5_4'], 'avg_pool5')
        # graph['fc6'] = self.__construct_fc_relu(graph['avg_pool5'], 37, 'fc6')
        # graph['fc7'] = self.__construct_fc_relu(graph['fc6'], 39, 'fc7')
        # graph['fc8'] = self.__construct_fc_relu(graph['fc7'], 41, 'fc8')
        # graph['prob'] = tf.nn.softmax(graph['fc8'], name='prob')
        self.graph = graph


    # input a img path list to test 
    def test(self, input_image):
        with tf.Session() as sess:
            prob = sess.run(self.graph['prob'],feed_dict={self.input_image:input_image})
            ans = tf.argmax(prob,1)
            ans = sess.run(ans)
            for a in ans:
                print a,self.synset[a]

    
def main():
    #vgg_path = download_vgg() 
    vgg_path = './imagenet-vgg-verydeep-19.mat'
    synset_path = './synset.ini'
    vgg = VGG_NET(vgg_path, synset_path)
    vgg.construct_vgg_net(batch_size=3)
    img_lists = ['./data/tiger.jpeg','./data/puzzle.jpeg','./data/deadpool.jpg']
    img_lists = fit_img(img_lists)
    # vgg.test(img_lists)
    with tf.Session() as sess:
        s = sess.run(vgg.graph['prob'], feed_dict={vgg.input_image:img_lists})
        ans = tf.argmax(s, 1)
        ans = sess.run(ans)
        for a in ans:
            print a, vgg.synset[a]


if __name__ == '__main__':
    main()